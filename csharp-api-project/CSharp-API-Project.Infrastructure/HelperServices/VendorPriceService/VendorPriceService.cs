using System.Reflection;
using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.Models.VendorPrice;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CSharpAPIProject.Infrastructure.HelperServices.VendorPriceService;

public class VendorPriceService : IVendorPriceService
{
    private readonly IApplicationDbContext _context;
    private ILogger<VendorPriceService> _logger;

    protected List<Supplier> suppliers = new List<Supplier>();
    protected Dictionary<string, decimal> RatesOfExchange = new Dictionary<string, decimal>();
    protected Dictionary<string,string> DebugLog = new Dictionary<string, string>();

    protected int DebugMainCounter = 1;
    protected int DebugSubCounter = 1;

    protected bool IsDebugging = false;

    public VendorPriceService(
        IApplicationDbContext context,
        ILogger<VendorPriceService> logger
    )
    {
        _context = context;
        _logger = logger;
    }

    #region Helpers

    private async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(fromCurrency))
            return 0;
        if (string.IsNullOrEmpty(toCurrency))
            return 0;

        // Check if the exchange rate combination exists
        var currencyPair = $"{fromCurrency}-{toCurrency}";
        decimal exchangeRate = 0;
        if (!RatesOfExchange.ContainsKey(currencyPair))
        {
            // If the currencies are the same then the rate of exchange is 1.
            if (fromCurrency == toCurrency)
                exchangeRate = 1;
            else
            {
                var exchangeRateRecord = await _context.RatesOfExchange
                    .Where(e => e.CurrencyPair == currencyPair)
                    .OrderByDescending(e => e.EffectiveDate)
                    .FirstOrDefaultAsync(cancellationToken);

                // If we didn't find the rate using the first currency pair then swap the currencies
                if (exchangeRateRecord == null)
                {
                    currencyPair = $"{toCurrency}-{fromCurrency}";
                    exchangeRateRecord = await _context.RatesOfExchange
                        .Where(e => e.CurrencyPair == currencyPair)
                        .OrderByDescending(e => e.EffectiveDate)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                if (exchangeRateRecord != null)
                {
                    RatesOfExchange.Add(currencyPair, exchangeRateRecord.ROE);
                    exchangeRate = exchangeRateRecord?.ROE ?? 0; // Needs to throw an exception if ROE is 0. Can't divide by 0.
                }
                else
                    exchangeRate = 0; // Needs to throw an exception for this product. Can't divide by 0.
            }
        }
        else RatesOfExchange.TryGetValue(currencyPair, out exchangeRate);

        return exchangeRate;
    }

    private async Task<Supplier> GetSupplier(int supplierId, CancellationToken cancellationToken)
    {
        Supplier supplier = null;

        if (!suppliers.Any(s => s.SupplierId == supplierId))
        {
            supplier = await _context.Suppliers.Where(s => s.SupplierId == supplierId).FirstOrDefaultAsync(cancellationToken);
            if (supplier != null)
                suppliers.Add(supplier);

            return supplier;
        } else supplier = suppliers.FirstOrDefault(s => s.SupplierId == supplierId);
        
        return supplier;
    }

    private void CalculatePrice(
        decimal SellingPrice,
        FeeType SupplierMarkupType,
        decimal SupplierMarkupValue,
        FeeType SupplierHandlingFeeType,
        decimal SupplierHandlingFeeValue,
        decimal ClientMarkupPercent,
        decimal ClientHandlingFee,
        bool ParticipantPaysHandlingFee,
        decimal ExchangeRate,
        out decimal itemTotal,
        out decimal HandlingFeeTotal,
        string debugReference = ""
    )
    {
        ClientMarkupPercent = ClientMarkupPercent / 100; // Convert percentage to decimal
        ClientHandlingFee = ClientHandlingFee / 100; // Convert percentage to decimal

        var supplierMarkupTotal = SupplierMarkupValue;
        if (SupplierMarkupType == Domain.Enums.FeeType.Percentage){
            SupplierMarkupValue = SupplierMarkupValue / 100;
            supplierMarkupTotal = SellingPrice * SupplierMarkupValue;
        }

        var supplierHandlingFeeTotal = SupplierHandlingFeeValue;
        if (SupplierHandlingFeeType == Domain.Enums.FeeType.Percentage){
            SupplierHandlingFeeValue = SupplierHandlingFeeValue / 100;
            supplierHandlingFeeTotal = SellingPrice * SupplierHandlingFeeValue;
        }

        AddDebugLog(debugReference,$"Selling Price * Supplier Markup Value = Supplier Markup Total");
        AddDebugLog(debugReference,$"{SellingPrice} * {supplierMarkupTotal} = {supplierMarkupTotal}");

        var ticMarkupTotal = SellingPrice * ClientMarkupPercent;
        var ticHandlingFeeTotal = SellingPrice * ClientHandlingFee;

        AddDebugLog(debugReference, $"Selling Price * Client Markup Percent = Markup Total");
        AddDebugLog(debugReference, $"{SellingPrice} * {ClientMarkupPercent} = {ticMarkupTotal}");

        AddDebugLog(debugReference, $"Selling Price * Client Handling Fee = Handling Fee Total");
        AddDebugLog(debugReference, $"{SellingPrice} * {ClientHandlingFee} = {ticHandlingFeeTotal}");

        decimal itemSubTotal = 0;
        decimal handlingFeeTotal = 0;
        // Does the participant of the loyalty program pay the Handling fee
        // or does the vendor of the loyalty program cover the handling fee?
        if (ParticipantPaysHandlingFee)
        {
            // The participant pays the handling fee
            itemSubTotal = SellingPrice +
                supplierMarkupTotal +
                supplierHandlingFeeTotal +
                ticHandlingFeeTotal +
                ticMarkupTotal;

            AddDebugLog(debugReference, $"Participant pays handling fee.");
            AddDebugLog(debugReference, $"Item Subtotal = Selling Price + Supplier Markup Total + Supplier Handling Fee Total + Handling Fee Total + Markup Total");
            AddDebugLog(debugReference, $"{SellingPrice} + {supplierMarkupTotal} + {supplierHandlingFeeTotal} + {ticHandlingFeeTotal} + {ticMarkupTotal} = {itemSubTotal}");
        }
        else
        {
            itemSubTotal = SellingPrice +
                supplierMarkupTotal +
                supplierHandlingFeeTotal +
                ticMarkupTotal;

            handlingFeeTotal = ticHandlingFeeTotal;

            AddDebugLog(debugReference, $"Client pays handling fee.");
            AddDebugLog(debugReference, $"Item Subtotal = Selling Price + Supplier Markup Total + Supplier Handling Fee Total + Markup Total");
            AddDebugLog(debugReference, $"{SellingPrice} + {supplierMarkupTotal} + {supplierHandlingFeeTotal} + {ticMarkupTotal} = {itemSubTotal}");

            AddDebugLog(debugReference, $"Handling Fee Total = Handling Fee Total");
            AddDebugLog(debugReference, $"{ticHandlingFeeTotal} = {handlingFeeTotal}");
        }

        itemTotal = itemSubTotal / ExchangeRate;
        HandlingFeeTotal = handlingFeeTotal / ExchangeRate;

        AddDebugLog(debugReference, $"Item Total = Item Subtotal / Exchange Rate");
        AddDebugLog(debugReference, $"{itemSubTotal} / {ExchangeRate} = {itemTotal}");

        AddDebugLog(debugReference, $"Handling Fee Total = Handling Fee Total / Exchange Rate");
        AddDebugLog(debugReference, $"{handlingFeeTotal} / {ExchangeRate} = {HandlingFeeTotal}");
    }

    public void StartDebugMode()
    {
        IsDebugging = true;
        DebugLog.Clear();
    }

    public void StopDebugMode()
    {
        IsDebugging = false;
    }

    public void AddDebugLog(string key, string message)
    {
        if (IsDebugging){
            DebugLog.Add($"{DebugMainCounter}.{DebugSubCounter}", $"{key}: {message}");

            DebugSubCounter++;
        }
    }

    #endregion Helpers

    public Dictionary<string, string> GetDebugLogs()
    {
        return DebugLog;
    }

    public async Task<ICollection<VendorProductPrice>> GetPrices(
        IList<VendorPrice> vendorPriceList,
        IAuthHelper authHelper,
        Vendor vendor,
        CancellationToken cancellationToken,
        bool EnableDebugLog = false
    )
    {
        if (EnableDebugLog) StartDebugMode();

        var vendorProductIds = vendorPriceList.Select(vp => vp.ProductId).ToList();
        var supplierPrices = await _context.SupplierPrices
            .Where(sp => vendorProductIds.Contains(sp.ProductId))
            .ToListAsync(cancellationToken);


        var vendorProductPrices = new List<VendorProductPrice>();
        var i = 0;

        AddDebugLog("Main Loop",$"Vendor = {vendor.Name}");
        AddDebugLog("Main Loop",$"{vendorPriceList.Count()} Price Lists");

        while (i < vendorPriceList.Count())
        {
            // Get Markups from the vendor price list
            var vendorPrice = vendorPriceList[i];

            if (EnableDebugLog) {
                AddDebugLog($"Price List {i + 1}",$"ProductId = {vendorPrice.ProductId}");
                AddDebugLog($"Price List {i + 1}",$"VendorId = {vendorPrice.VendorId}");
                AddDebugLog($"Price List {i + 1}", $"RegionId = {vendorPrice.RegionId}");
            }

            Supplier supplier = await GetSupplier(vendorPrice.Product.SupplierId, cancellationToken);
            decimal exchangeRate = await GetExchangeRate(supplier.QuoteCurrency.Code, vendorPrice.VendorCurrency.Code, cancellationToken);

            if(exchangeRate == 0)
            {
                AddDebugLog($"Price List {i + 1}",$"Exchange rate not found.");
                AddDebugLog($"Price List {i + 1}",$"Supplier Currency = {supplier.QuoteCurrency.Code}");
                AddDebugLog($"Price List {i + 1}",$"Vendor Currency = {vendorPrice.VendorCurrency.Code}");
                AddDebugLog($"Price List {i + 1}",$"ProductId = {vendorPrice.ProductId}");
                AddDebugLog($"Price List {i + 1}",$"VendorId = {vendorPrice.VendorId}");
                AddDebugLog($"Price List {i + 1}",$"SupplierId = {vendorPrice.Product.SupplierId}");
                AddDebugLog($"Price List {i + 1}",$"RegionId = {vendorPrice.RegionId}");

                _logger.LogError("Exchange rate not found for [{FromCurrency}] to [{ToCurrency}]. ProductId = [{ProductId}], VendorId = [{VendorId}], SupplierId = [{SupplierId}], RegionId = [{RegionId}]",
                    supplier.QuoteCurrency.Code,
                    vendorPrice.VendorCurrency.Code,
                    vendorPrice.ProductId,
                    vendorPrice.VendorId,
                    vendorPrice.Product.SupplierId,
                    vendorPrice.RegionId);
                i++; // Move on to the next record.
                continue;
            } else
            {
                AddDebugLog($"Price List {i + 1}", $"Exchange Rate = {exchangeRate} - {supplier.QuoteCurrency.Code} to {vendorPrice.VendorCurrency.Code}");
            }

            // Make an empty object to map totals to
            VendorProductPrice vendorProductPrice = new VendorProductPrice();

            // Get the supplier prices for the product
            var supplierProductPrices = supplierPrices.Where(sp => sp.ProductId == vendorPrice.ProductId).ToList();

            var modelVariants = vendorPrice.Product.Model?.Variants;
            vendorProductPrice.Variants = new List<VendorProductVariantModel>();

            var baseProductPrice = supplierProductPrices.Where(sp => sp.SKU == vendorPrice.Product.Sku).FirstOrDefault();

            if (modelVariants != null)
            {
                AddDebugLog($"Price List {i + 1}", $"{modelVariants.Count} variants to calculate prices for.");
                foreach (var variant in modelVariants)
                {
                    DebugMainCounter += 1;
                    DebugSubCounter = 0;
                    AddDebugLog(variant.Sku,$"Calculating Price");

                    // Create a new instance of VendorProductVariantModel
                    VendorProductVariantModel vendorVariant = new VendorProductVariantModel();

                    // Copy properties from variant to vendorVariant
                    foreach (PropertyInfo property in variant.GetType().GetProperties())
                    {
                        PropertyInfo vendorProperty = vendorVariant.GetType().GetProperty(property.Name);
                        if (vendorProperty != null && vendorProperty.CanWrite)
                            vendorProperty.SetValue(vendorVariant, property.GetValue(variant));
                    }

                    // Get the price for the variant
                    var supplierVariantPrice = supplierProductPrices.FirstOrDefault(sp => sp.SKU == variant.Sku);

                    // Now that we have the price, we need to calculate the total
                    var basePrice = supplierVariantPrice?.SellingPrice ?? 0;
                    var supplierMarkupType = supplierVariantPrice?.MarkupType ?? Domain.Enums.FeeType.Flat;
                    var supplierMarkupValue = supplierVariantPrice?.MarkupValue ?? 0;
                    var supplierHandlingFeeType = supplierVariantPrice?.HandlingFeeType ?? Domain.Enums.FeeType.Flat;
                    var supplierHandlingFeeValue = supplierVariantPrice?.HandlingFeeValue ?? 0;

                    AddDebugLog(variant.Sku, $"Base Price {basePrice}");
                    AddDebugLog(variant.Sku, $"Supplier Markup Type = {supplierMarkupType}, Value = {supplierMarkupValue}");
                    AddDebugLog(variant.Sku, $"Supplier Handling Fee Type = {supplierHandlingFeeType}, Value = {supplierHandlingFeeValue}");
                    AddDebugLog(variant.Sku, $"Client Markup % = {vendorPrice.ClientMarkupPercent}");
                    AddDebugLog(variant.Sku, $"Client Handling Fee = {vendorPrice.ClientHandlingFee}");
                    AddDebugLog(variant.Sku, $"Who pays handling fee = " + (vendor.PariticpantPaysHandlingFee ? "Participant" : "Client"));

                    CalculatePrice(
                        basePrice,
                        supplierMarkupType,
                        supplierMarkupValue,
                        supplierHandlingFeeType,
                        supplierHandlingFeeValue,
                        vendorPrice.ClientMarkupPercent,
                        vendorPrice.ClientHandlingFee,
                        vendor.PariticpantPaysHandlingFee,
                        exchangeRate,
                        out decimal variantGrandTotal, 
                        out decimal variantHandlingFeeTotal,
                        variant.Sku
                    );

                    AddDebugLog(variant.Sku, $"Variant Grand Total = {variantGrandTotal}");
                    AddDebugLog(variant.Sku, $"Variant Handling Fee Total = {variantHandlingFeeTotal}");

                    vendorVariant.PriceTotal = variantGrandTotal;
                    vendorVariant.HandlingFee = variantHandlingFeeTotal;

                    vendorProductPrice.Variants.Add(vendorVariant);
                }
            } else 
                AddDebugLog($"Price List {i + 1}",$"No variants found.");

            vendorProductPrice.Product = new VendorProductModel();
            foreach (PropertyInfo property in vendorPrice.Product.Model.GetType().GetProperties())
            {
                PropertyInfo vendorProperty = vendorProductPrice.Product.GetType().GetProperty(property.Name);
                if (vendorProperty != null && vendorProperty.CanWrite)
                    vendorProperty.SetValue(vendorProductPrice.Product, property.GetValue(vendorPrice.Product.Model));
            }

            DebugMainCounter += 1;
            DebugSubCounter = 0;

            // Calculate the product price
            var baseProductSellingPrice = baseProductPrice.SellingPrice;
            FeeType supplierProductMarkupType = baseProductPrice.MarkupType;
            var supplierProductMarkupValue = baseProductPrice.MarkupValue;
            FeeType supplierProductHandlingFeeType = baseProductPrice.HandlingFeeType;
            var supplierProductHandlingFeeValue = baseProductPrice.HandlingFeeValue;

            AddDebugLog($"Price List {i + 1}", $"Calculating Base Product Price");
            AddDebugLog($"Price List {i + 1}", $"Base Product Selling Price = {baseProductSellingPrice}");
            AddDebugLog($"Price List {i + 1}", $"Supplier Product Markup Type = {supplierProductMarkupType}, Value = {supplierProductMarkupValue}");
            AddDebugLog($"Price List {i + 1}", $"Supplier Product Handling Fee Type = {supplierProductHandlingFeeType}, Value = {supplierProductHandlingFeeValue}");

            AddDebugLog($"Price List {i + 1}", $"Client Markup % = {vendorPrice.ClientMarkupPercent}");
            AddDebugLog($"Price List {i + 1}", $"Client Handling Fee = {vendorPrice.ClientHandlingFee}");

            CalculatePrice(baseProductSellingPrice,
                supplierProductMarkupType,
                supplierProductMarkupValue,
                supplierProductHandlingFeeType,
                supplierProductHandlingFeeValue,
                vendorPrice.ClientMarkupPercent,
                vendorPrice.ClientHandlingFee,
                vendor.PariticpantPaysHandlingFee,
                exchangeRate,
                out decimal productGrandTotal,
                out decimal productHandlingFeeTotalFinal,
                $"Price List {i + 1}"
            );

            AddDebugLog($"Price List {i + 1}", $"Product Grand Total = {productGrandTotal}");
            AddDebugLog($"Price List {i + 1}", $"Product Handling Fee Total = {productHandlingFeeTotalFinal}");

            vendorProductPrice.Product.PriceTotal = productGrandTotal;
            vendorProductPrice.Product.HandlingFee = productHandlingFeeTotalFinal;
            vendorProductPrice.ProductId = vendorPrice.ProductId;

            if(EnableDebugLog) {
                //vendorProductPrice.DebugData = GetDebugLogs();
                vendorProductPrice.HasDebugData = true;
                vendorProductPrice.Debug = new VendorProductPriceDebugData()
                {
                    Log = GetDebugLogs(),
                    HandlingFeeType = supplierProductHandlingFeeType,
                    HandlingFeeValue = supplierProductHandlingFeeValue,
                    MarkupType = supplierProductMarkupType,
                    MarkupValue = supplierProductMarkupValue,
                };
            }

            bool isTicAdmin = await authHelper.LoggedIn_IsTICUser();
            //if the logged in user is a Admin then we can add the client handling fee and markup percentage to the debug data
            if (isTicAdmin)
            {
                if (EnableDebugLog)
                {
                    vendorProductPrice.Debug.ClientHandlingFeeValue = vendorPrice.ClientHandlingFee;
                    vendorProductPrice.Debug.ClientMarkupPercentageValue = vendorPrice.ClientMarkupPercent;
                }
            }

            vendorProductPrices.Add(vendorProductPrice);
            i++;
            DebugMainCounter += 1;
            DebugSubCounter = 0;
        }

        return vendorProductPrices;
    }
}
