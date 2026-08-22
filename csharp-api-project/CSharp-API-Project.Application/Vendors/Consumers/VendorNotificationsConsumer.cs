using MassTransit;
using Microsoft.Extensions.Logging;
using CSharpAPIProject.Application.Common.Interfaces;
//using CSharpAPIProject.Application.Prices.Models;
using CSharpAPIProject.Application.Products.Models;
//using CSharpAPIProject.Application.Stock.Models;
using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Domain.Contracts;
using CSharpAPIProject.Domain.Entities;
using System.Text.Json;

namespace CSharpAPIProject.Application.Vendors.Consumers
{
	//public class VendorNotificationsConsumer(IApplicationDbContext context, IBus bus, IMapper mapper, ILogger<VendorNotificationsConsumer> Logger) : IConsumer<Batch<VendorNotificationPayload>>
	//{
	//	public async Task Consume(ConsumeContext<Batch<VendorNotificationPayload>> consumerContext)
	//	{
	//		var payloadItems = consumerContext.Message.Select(x => new VendorNotificationPayload()
	//		{
	//			PriceChange = x.Message.PriceChange,
	//			StockLevelChange = x.Message.StockLevelChange,
	//			ProductChange = x.Message.ProductChange
	//		}).ToList();

	//		var webhookPayloads = await ProcessBatch(payloadItems);

	//		// Publish to WebhookNotificationConsumer
	//		foreach (var webhookPayload in webhookPayloads)
	//		{
	//			await bus.Publish(webhookPayload);
	//		}
	//	}
	//	public async Task<List<WebhookNotificationBatchPayload>> ProcessBatch(List<VendorNotificationPayload> payloadItems)
	//	{
	//		var webhookPayloads = new List<WebhookNotificationBatchPayload>();
	//		// Extract entities and ids from the payload
	//		var prices = payloadItems.Where(x => x.PriceChange != null).Select(x => x.PriceChange).ToList();
	//		var stockLevels = payloadItems.Where(x => x.StockLevelChange != null).Select(x => x.StockLevelChange).ToList();
	//		var products = payloadItems.Where(x => x.ProductChange != null).Select(x => x.ProductChange).ToList();

	//		var pricelistCodes = prices.Select(x => x.PricelistCode);
	//		var stockLocationCodes = stockLevels.Select(x => x.StockLocationCode);

	//		var supplierCodes = prices.Select(x => x.SupplierCode).Union(stockLevels.Select(y => y.SupplierCode).Union(products.Select(z => z.SupplierCode))).Distinct().ToList();
	//		var suppliersQuery = context.Suppliers.Where(x => supplierCodes.Contains(x.Code));
	//		var suppliers = await suppliersQuery.ToListAsync();
	//		var supplierIds = suppliers.Select(x => x.SupplierId).ToList();

	//		var productSkus = products.Select(x => x.Sku).ToList();

	//		// Get all feeds associated with the Price, Stocklevels and Products in this batch

	//		var feedsQuery = context.Feeds.Include(x => x.ApprovedPricelists).Include(x => x.ApprovedStockLocations).Include(x => x.Vendor)
	//			.Where(feed =>
	//				(feed.ApprovedPricelists.Any(pricelist => pricelistCodes.Contains(pricelist.PricelistCode)) ||
	//				feed.ApprovedStockLocations.Any(stockLocation => stockLocationCodes.Contains(stockLocation.LocationCode)))
	//				&& supplierIds.Contains(feed.SupplierId)
	//				);
	//		//// Since Products are a function of pricelist and prices, we need to figure out which products are related to approved feed pricelists
	//		List<TempFeed> tempProductFeeds = [];
	//		if (productSkus.Count > 0)
	//		{
	//			string supplierIdsStr = string.Join(",", supplierIds);
	//			string productSkusStr = string.Join(",", productSkus.Select(x => $"'{x}'"));
	//			var productFeedsQuery = ((context as DbContext)!).Database.SqlQueryRaw<TempFeed>($@"SELECT DISTINCT
	//			f.FeedId, f.VendorId, f.SupplierId, f.FeedCode, fpl.ApprovedPricelistsPricelistId, p.sku, v.WebhookUrl, v.WebhookSecret
	//			FROM Feed f 
	//			INNER JOIN FeedPricelist fpl ON f.FeedId = fpl.FeedId 
	//			INNER JOIN Pricelist pl ON fpl.ApprovedPricelistsPricelistId = pl.PricelistId 
	//			INNER JOIN Price p ON p.SupplierId IN ({supplierIdsStr}) AND pl.PricelistId = p.PricelistId JOIN Vendor v ON v.VendorId = f.VendorId WHERE p.Sku in ({productSkusStr});");

	//			tempProductFeeds = await productFeedsQuery.ToListAsync();
	//			tempProductFeeds = tempProductFeeds.Where(x => supplierIds.Contains(x.SupplierId)).ToList();
	//		}

	//		var feeds = await feedsQuery.ToListAsync();
	//		var productFeeds = tempProductFeeds.Distinct().Select(x => new Feed
	//		{
	//			FeedId = x.FeedId,
	//			VendorId = x.VendorId,
	//			SupplierId = x.SupplierId,
	//			FeedCode = x.FeedCode,
	//			ApprovedPricelists = tempProductFeeds.Distinct().Where(y => y.FeedId == x.FeedId).Select(z => new Pricelist { PricelistId = x.ApprovedPricelistsPricelistId }).ToList(),
	//			Vendor = new Vendor { VendorId = x.VendorId, WebhookUrl = x.WebhookUrl, WebhookSecret = x.WebhookSecret }
	//		});

	//		// Combine the price and stocklevel feeds with the product feeds
	//		feeds = feeds.UnionBy(productFeeds, x => x.FeedId).ToList();
	//		var vendorSkuMapping = tempProductFeeds.Select(x => new { x.VendorId, x.Sku }).ToList();

	//		//// Group feeds by vendor and iterate through each vendor to build their payload
	//		var vendors = feeds.Select(x => x.Vendor).DistinctBy(x => x.VendorId).ToList();
	//		foreach (var vendor in vendors)
	//		{
	//			// Get this vendor's feeds
	//			var vendorFeeds = feeds.Where(x => x.VendorId == vendor.VendorId).ToList();

	//			// Extract vendor prices, stocklevels and products and project to Dtos
	//			var vendorPrices = prices.Where(x => vendorFeeds.SelectMany(y => y.ApprovedPricelists).Select(z => z.PricelistCode).Contains(x.PricelistCode))
	//				.AsQueryable().ProjectTo<PriceUpdateLogDto>(mapper.ConfigurationProvider).ToList();

	//			var vendorStockLevels = stockLevels.Where(x => vendorFeeds.SelectMany(y => y.ApprovedStockLocations).Select(z => z.LocationCode).Contains(x.StockLocationCode))
	//				.AsQueryable().ProjectTo<StockLevelUpdateLogDto>(mapper.ConfigurationProvider).ToList();

	//			var vendorSkus = vendorSkuMapping.Where(x => x.VendorId == vendor.VendorId).Select(x => x.Sku).ToList();
	//			var vendorProducts = products.Where(x => vendorSkus.Contains(x.Sku)).AsQueryable().ProjectTo<ProductUpdateLogDto>(mapper.ConfigurationProvider).ToList();

	//			//var webhookPayload = new VendorWebhookPayloadDto { Prices = vendorPrices, Products = vendorProducts, StockLevels = vendorStockLevels };
	//			// We break the payload into batches of 500 items to keep size in check.
	//			var webhookPayloadBatches = CreateWebhookPayloadBatches(vendorPrices, vendorProducts, vendorStockLevels, vendor.WebhookUrl, vendor.WebhookSecret);
	//			foreach (var batch in webhookPayloadBatches)
	//			{
	//				// Serialize the payload batches
	//				var webhookPayloadSerialized = JsonSerializer.Serialize(batch);

	//				// Add to the list of payloads
	//				webhookPayloads.Add(new WebhookNotificationBatchPayload
	//				{
	//					Payload = webhookPayloadSerialized,
	//					WebhookUrl = vendor.WebhookUrl,
	//					WebhookSecret = vendor.WebhookSecret
	//				});

	//			}


	//		}
	//		return webhookPayloads;
	//	}
	//	public List<WebhookNotificationBatchPayload> CreateWebhookPayloadBatches(
	//List<PriceUpdateLogDto> prices,
	//List<ProductUpdateLogDto> products,
	//List<StockLevelUpdateLogDto> stockLevels,
	//string webhookUrl,
	//string webhookSecret)
	//	{
	//		var webhookPayloads = new List<WebhookNotificationBatchPayload>();

	//		// Flatten the payload
	//		var allItems = new List<WebhookItem>();
	//		allItems.AddRange(prices.Select(p => new WebhookItem { Type = "Price", Item = p }));
	//		allItems.AddRange(products.Select(p => new WebhookItem { Type = "Product", Item = p }));
	//		allItems.AddRange(stockLevels.Select(sl => new WebhookItem { Type = "StockLevel", Item = sl }));

	//		int batchSize = 500;
	//		for (int i = 0; i < allItems.Count; i += batchSize)
	//		{
	//			var batchItems = allItems.Skip(i).Take(batchSize).ToList();

	//			// Unflatten the batch into a new VendorWebhookPayloadDto
	//			var batchPrices = batchItems.Where(item => item.Type == "Price").Select(item => (PriceUpdateLogDto)item.Item).ToList();
	//			var batchProducts = batchItems.Where(item => item.Type == "Product").Select(item => (ProductUpdateLogDto)item.Item).ToList();
	//			var batchStockLevels = batchItems.Where(item => item.Type == "StockLevel").Select(item => (StockLevelUpdateLogDto)item.Item).ToList();

	//			var webhookPayload = new VendorWebhookPayloadDto
	//			{
	//				Prices = batchPrices,
	//				Products = batchProducts,
	//				StockLevels = batchStockLevels
	//			};

	//			var webhookPayloadSerialized = JsonSerializer.Serialize(webhookPayload);

	//			webhookPayloads.Add(new WebhookNotificationBatchPayload
	//			{
	//				Payload = webhookPayloadSerialized,
	//				WebhookUrl = webhookUrl,
	//				WebhookSecret = webhookSecret
	//			});
	//		}

	//		return webhookPayloads;
	//	}
	//	public class WebhookItem
	//	{
	//		public string Type { get; set; } // "Price", "Product", or "StockLevel"
	//		public object Item { get; set; }
	//	}
	}
	//public class TempFeed
	//{
	//	public int FeedId { get; set; }
	//	public int VendorId { get; set; }
	//	public int SupplierId { get; set; }
	//	public string FeedCode { get; set; }
	//	public int ApprovedPricelistsPricelistId { get; set; }
	//	public string Sku { get; set; }
	//	public string WebhookUrl { get; set; }
	//	public string WebhookSecret { get; set; }
	//}
	//public class VendorNotificationPayload
	//{
	//	public PriceChangeLog? PriceChange { get; set; }
	//	public ProductChangeLog? ProductChange { get; set; }
	//	public StockLevelChangeLog? StockLevelChange { get; set; }
	//}
	//public class VendorWebhookPayloadDto
	//{
	//	public List<PriceUpdateLogDto> Prices { get; set; } = [];
	//	public List<ProductUpdateLogDto> Products { get; set; } = [];
	//	public List<StockLevelUpdateLogDto> StockLevels { get; set; } = [];
	//}
//}
