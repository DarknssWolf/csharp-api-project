using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharpAPIProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGroup",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroup", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "Vendor",
                columns: table => new
                {
                    VendorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    WebhookSecret = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FinanceEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor", x => x.VendorId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubGroup",
                columns: table => new
                {
                    SubGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubGroup", x => x.SubGroupId);
                    table.ForeignKey(
                        name: "FK_UserSubGroup_UserGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "UserGroup",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Region",
                columns: table => new
                {
                    RegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RegionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.RegionId);
                    table.ForeignKey(
                        name: "FK_Region_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoredFile",
                columns: table => new
                {
                    StoredFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFile", x => x.StoredFileId);
                    table.ForeignKey(
                        name: "FK_StoredFile_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Survey",
                columns: table => new
                {
                    SurveyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CanEarnPoints = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowMultipleTries = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MultipleTriesLimit = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Points = table.Column<int>(type: "int", nullable: false),
                    SurveyData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordStatus = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    RecordDeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Survey", x => x.SurveyId);
                    table.ForeignKey(
                        name: "FK_Survey_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorLanguage",
                columns: table => new
                {
                    VendorLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ISOCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorLanguage", x => x.VendorLanguageId);
                    table.ForeignKey(
                        name: "FK_VendorLanguage_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoteSet",
                columns: table => new
                {
                    VoteSetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CanEarnPoints = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowMultipleTries = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowMultipleItems = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailNotification = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VoteSetData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhoCanCreateVoteItemFlag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteSet", x => x.VoteSetId);
                    table.ForeignKey(
                        name: "FK_VoteSet_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    VendorRegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IDNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Address_Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Street2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Province = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordStatus = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    RecordDeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customer_Region_VendorRegionId",
                        column: x => x.VendorRegionId,
                        principalTable: "Region",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customer_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SurveyQuestion",
                columns: table => new
                {
                    SurveyQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    ParentQuestionId = table.Column<int>(type: "int", nullable: true),
                    Prompt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedAnswers = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AllowMultipleResponses = table.Column<bool>(type: "bit", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ParentCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SurveyQuestionData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyQuestion", x => x.SurveyQuestionId);
                    table.ForeignKey(
                        name: "FK_SurveyQuestion_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "SurveyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoteItem",
                columns: table => new
                {
                    VoteItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoteSetId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoteItemStatusReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VoteItemData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteItem", x => x.VoteItemId);
                    table.ForeignKey(
                        name: "FK_VoteItem_VoteSet_VoteSetId",
                        column: x => x.VoteSetId,
                        principalTable: "VoteSet",
                        principalColumn: "VoteSetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SurveyResponseSets",
                columns: table => new
                {
                    SurveyResponseSetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedQuestions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastQuestionIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ResponseSetStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponseSets", x => x.SurveyResponseSetId);
                    table.ForeignKey(
                        name: "FK_SurveyResponseSets_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SurveyResponseSets_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "SurveyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SurveyResponseSets_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoteSubmission",
                columns: table => new
                {
                    VoteSubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoteItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteSubmission", x => x.VoteSubmissionId);
                    table.ForeignKey(
                        name: "FK_VoteSubmission_VoteItem_VoteItemId",
                        column: x => x.VoteItemId,
                        principalTable: "VoteItem",
                        principalColumn: "VoteItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SurveyResponse",
                columns: table => new
                {
                    SurveyResponseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    SurveyId = table.Column<int>(type: "int", nullable: false),
                    SurveyQuestionId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SurveyResponseSetId = table.Column<int>(type: "int", nullable: true),
                    Response = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_Status = table.Column<int>(type: "int", nullable: true),
                    StatusFlag_StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFlag_StatusUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VisibilityConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponse", x => x.SurveyResponseId);
                    table.ForeignKey(
                        name: "FK_SurveyResponse_SurveyQuestion_SurveyQuestionId",
                        column: x => x.SurveyQuestionId,
                        principalTable: "SurveyQuestion",
                        principalColumn: "SurveyQuestionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SurveyResponse_SurveyResponseSets_SurveyResponseSetId",
                        column: x => x.SurveyResponseSetId,
                        principalTable: "SurveyResponseSets",
                        principalColumn: "SurveyResponseSetId");
                    table.ForeignKey(
                        name: "FK_SurveyResponse_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "SurveyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                columns: new[] { "NormalizedEmail", "TenantId" },
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email_TenantId",
                table: "AspNetUsers",
                columns: new[] { "Email", "TenantId" },
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                columns: new[] { "NormalizedUserName", "TenantId" },
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_VendorId",
                table: "Customer",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_VendorRegionId",
                table: "Customer",
                column: "VendorRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Region_VendorId_RegionCode",
                table: "Region",
                columns: new[] { "VendorId", "RegionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFile_VendorId",
                table: "StoredFile",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_VendorId",
                table: "Survey",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestion_SurveyId",
                table: "SurveyQuestion",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestion_SurveyId_QuestionId_ParentQuestionId",
                table: "SurveyQuestion",
                columns: new[] { "SurveyId", "QuestionId", "ParentQuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_SurveyId",
                table: "SurveyResponse",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_SurveyQuestionId",
                table: "SurveyResponse",
                column: "SurveyQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_SurveyResponseSetId",
                table: "SurveyResponse",
                column: "SurveyResponseSetId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseSets_CustomerId",
                table: "SurveyResponseSets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseSets_SurveyId",
                table: "SurveyResponseSets",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseSets_VendorId",
                table: "SurveyResponseSets",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseSets_VendorId_SurveyId_CustomerId",
                table: "SurveyResponseSets",
                columns: new[] { "VendorId", "SurveyId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubGroup_GroupId",
                table: "UserSubGroup",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_Code",
                table: "Vendor",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorLanguage_VendorId",
                table: "VendorLanguage",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteItem_VoteSetId",
                table: "VoteItem",
                column: "VoteSetId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteSet_VendorId",
                table: "VoteSet",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteSubmission_VoteItemId",
                table: "VoteSubmission",
                column: "VoteItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "StoredFile");

            migrationBuilder.DropTable(
                name: "SurveyResponse");

            migrationBuilder.DropTable(
                name: "UserSubGroup");

            migrationBuilder.DropTable(
                name: "VendorLanguage");

            migrationBuilder.DropTable(
                name: "VoteSubmission");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "SurveyQuestion");

            migrationBuilder.DropTable(
                name: "SurveyResponseSets");

            migrationBuilder.DropTable(
                name: "UserGroup");

            migrationBuilder.DropTable(
                name: "VoteItem");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Survey");

            migrationBuilder.DropTable(
                name: "VoteSet");

            migrationBuilder.DropTable(
                name: "Region");

            migrationBuilder.DropTable(
                name: "Vendor");
        }
    }
}
