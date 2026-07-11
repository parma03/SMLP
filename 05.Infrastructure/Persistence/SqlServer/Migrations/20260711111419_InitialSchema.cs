using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smlp.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Master");

            migrationBuilder.EnsureSchema(
                name: "Common");

            migrationBuilder.EnsureSchema(
                name: "Transaction");

            migrationBuilder.CreateTable(
                name: "AppSetting",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Group = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Audit",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientApplicationId = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    FromIpAddress = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    FromGeolocation_Latitude = table.Column<double>(type: "float", nullable: true),
                    FromGeolocation_Longitude = table.Column<double>(type: "float", nullable: true),
                    FromGeolocation_Accuracy = table.Column<double>(type: "float", nullable: true),
                    FromGeolocation_City = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    FromGeolocation_Country = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    FileContentType = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StorageFileId = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointRule",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    SpendAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PointEarned = table.Column<int>(type: "int", nullable: false),
                    BonusMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointRule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategory",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductImage",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    FileContentType = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StorageFileId = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RewardImage",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    FileContentType = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StorageFileId = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardImage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TierRule",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    MinimumLifetimePoints = table.Column<int>(type: "int", nullable: false),
                    MaximumLifetimePoints = table.Column<int>(type: "int", nullable: false),
                    PointMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BenefitDescription = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    BadgeColor = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TierRule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    ProductImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_ProductCategory_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Master",
                        principalTable: "ProductCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_ProductImage_ProductImageId",
                        column: x => x.ProductImageId,
                        principalSchema: "Master",
                        principalTable: "ProductImage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reward",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    PointCost = table.Column<int>(type: "int", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FreeItemName = table.Column<string>(type: "nvarchar(150)", nullable: true),
                    StockLimit = table.Column<int>(type: "int", nullable: true),
                    RedeemedCount = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumTier = table.Column<int>(type: "int", nullable: false),
                    RewardImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reward_RewardImage_RewardImageId",
                        column: x => x.RewardImageId,
                        principalSchema: "Master",
                        principalTable: "RewardImage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberCode = table.Column<string>(type: "nvarchar(60)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(60)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(150)", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Tier = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    LifetimePoints = table.Column<int>(type: "int", nullable: false),
                    TotalSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastTransactionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointAdjustment",
                schema: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    AdjustmentType = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    AdjustmentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointAdjustment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointAdjustment_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Master",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RedeemHistory",
                schema: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RedeemCode = table.Column<string>(type: "nvarchar(60)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RewardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RedeemDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PointsUsed = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    VerifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    RewardNameSnapshot = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    PointCostSnapshot = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedeemHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedeemHistory_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Master",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RedeemHistory_Reward_RewardId",
                        column: x => x.RewardId,
                        principalSchema: "Master",
                        principalTable: "Reward",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                schema: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionCode = table.Column<string>(type: "nvarchar(60)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    BonusPoints = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    CustomerTierSnapshot = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    PointMultiplierSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Master",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionItem",
                schema: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Master",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionItem_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Transaction",
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionPayment",
                schema: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(60)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionPayment_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Transaction",
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(60)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(60)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProfilePictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfilePicture",
                schema: "Master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    FileContentType = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StorageFileId = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfilePicture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfilePicture_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Master",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSetting_Key",
                schema: "Master",
                table: "AppSetting",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CreatedAt",
                schema: "Master",
                table: "Customer",
                column: "CreatedAt",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_LastTransactionDate",
                schema: "Master",
                table: "Customer",
                column: "LastTransactionDate",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_MemberCode",
                schema: "Master",
                table: "Customer",
                column: "MemberCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PhoneNumber",
                schema: "Master",
                table: "Customer",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Tier",
                schema: "Master",
                table: "Customer",
                column: "Tier",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_TotalSpend",
                schema: "Master",
                table: "Customer",
                column: "TotalSpend",
                filter: "[IsDeleted] = 0 AND [TotalSpend] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_UserId",
                schema: "Master",
                table: "Customer",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentType",
                schema: "Common",
                table: "Document",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_PointAdjustment_AdjustmentDate",
                schema: "Transaction",
                table: "PointAdjustment",
                column: "AdjustmentDate",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PointAdjustment_CustomerId_AdjustmentDate",
                schema: "Transaction",
                table: "PointAdjustment",
                columns: new[] { "CustomerId", "AdjustmentDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PointRule_IsActive",
                schema: "Master",
                table: "PointRule",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PointRule_ValidFrom_ValidUntil",
                schema: "Master",
                table: "PointRule",
                columns: new[] { "ValidFrom", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_Product_IsActive",
                schema: "Master",
                table: "Product",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Name",
                schema: "Master",
                table: "Product",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductId",
                schema: "Master",
                table: "Product",
                column: "ProductId",
                unique: true,
                filter: "[ProductId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductImageId",
                schema: "Master",
                table: "Product",
                column: "ProductImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_IsActive",
                schema: "Master",
                table: "ProductCategory",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_Name",
                schema: "Master",
                table: "ProductCategory",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedeemHistory_CustomerId_RedeemDate",
                schema: "Transaction",
                table: "RedeemHistory",
                columns: new[] { "CustomerId", "RedeemDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RedeemHistory_RedeemCode",
                schema: "Transaction",
                table: "RedeemHistory",
                column: "RedeemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedeemHistory_RedeemDate",
                schema: "Transaction",
                table: "RedeemHistory",
                column: "RedeemDate",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RedeemHistory_RewardId",
                schema: "Transaction",
                table: "RedeemHistory",
                column: "RewardId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RedeemHistory_Status_RedeemDate",
                schema: "Transaction",
                table: "RedeemHistory",
                columns: new[] { "Status", "RedeemDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Reward_IsActive",
                schema: "Master",
                table: "Reward",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Reward_PointCost",
                schema: "Master",
                table: "Reward",
                column: "PointCost");

            migrationBuilder.CreateIndex(
                name: "IX_Reward_RewardImageId",
                schema: "Master",
                table: "Reward",
                column: "RewardImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Reward_ValidFrom_ValidUntil",
                schema: "Master",
                table: "Reward",
                columns: new[] { "ValidFrom", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_TierRule_IsActive",
                schema: "Master",
                table: "TierRule",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TierRule_Tier",
                schema: "Master",
                table: "TierRule",
                column: "Tier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CustomerId_TransactionDate",
                schema: "Transaction",
                table: "Transaction",
                columns: new[] { "CustomerId", "TransactionDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Status_TransactionDate",
                schema: "Transaction",
                table: "Transaction",
                columns: new[] { "Status", "TransactionDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionCode",
                schema: "Transaction",
                table: "Transaction",
                column: "TransactionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionDate",
                schema: "Transaction",
                table: "Transaction",
                column: "TransactionDate",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItem_ProductId",
                schema: "Transaction",
                table: "TransactionItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItem_TransactionId",
                schema: "Transaction",
                table: "TransactionItem",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayment_TransactionId",
                schema: "Transaction",
                table: "TransactionPayment",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_User_EmailAddress",
                schema: "Master",
                table: "User",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_IsActive",
                schema: "Master",
                table: "User",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_User_ProfilePictureId",
                schema: "Master",
                table: "User",
                column: "ProfilePictureId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                schema: "Master",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfilePicture_UserId",
                schema: "Master",
                table: "UserProfilePicture",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_User_UserId",
                schema: "Master",
                table: "Customer",
                column: "UserId",
                principalSchema: "Master",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_UserProfilePicture_ProfilePictureId",
                schema: "Master",
                table: "User",
                column: "ProfilePictureId",
                principalSchema: "Master",
                principalTable: "UserProfilePicture",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfilePicture_User_UserId",
                schema: "Master",
                table: "UserProfilePicture");

            migrationBuilder.DropTable(
                name: "AppSetting",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "Audit",
                schema: "Common");

            migrationBuilder.DropTable(
                name: "Document",
                schema: "Common");

            migrationBuilder.DropTable(
                name: "PointAdjustment",
                schema: "Transaction");

            migrationBuilder.DropTable(
                name: "PointRule",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "RedeemHistory",
                schema: "Transaction");

            migrationBuilder.DropTable(
                name: "TierRule",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "TransactionItem",
                schema: "Transaction");

            migrationBuilder.DropTable(
                name: "TransactionPayment",
                schema: "Transaction");

            migrationBuilder.DropTable(
                name: "Reward",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "Transaction",
                schema: "Transaction");

            migrationBuilder.DropTable(
                name: "RewardImage",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "ProductCategory",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "ProductImage",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "Customer",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Master");

            migrationBuilder.DropTable(
                name: "UserProfilePicture",
                schema: "Master");
        }
    }
}
