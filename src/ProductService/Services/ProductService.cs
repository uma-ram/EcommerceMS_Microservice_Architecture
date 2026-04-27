
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
using System.Text.Json;

namespace ProductService.Services;

public class ProductCatalogService
{
    private readonly ProductDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

    public ProductCatalogService(ProductDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        const string cacheKey = "products:all";        
        //1. Check  redis first

        var cached = await _cache.GetStringAsync(cacheKey);
        if ( cached is not null)
        {
            Console.WriteLine(">>>Cache Hit for Products:all");
            return JsonSerializer.Deserialize<List<ProductResponse>>(cached);
        }

        //2. Cache miss - go to DB
        Console.WriteLine(">>> Cache MISS for products:all");
        var products = await _db.Products
            .Where(p=>p.IsActive)
            .Select(p=>ToResponse(p))
            .ToListAsync();

        //3. Store in Redis for next time
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheDuration });

        return products;
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var cacheKey = $"products:{id}";

        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            Console.WriteLine($">>> Cache HIT for products:{id}");
            return JsonSerializer.Deserialize<ProductResponse>(cached);
        }

        Console.WriteLine($">>> Cache MISS for products:{id}");
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive); 

        if (product is null) return null;

        var response = ToResponse(product);
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response),
            new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = _cacheDuration 
            });

        return response;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        var cached = await _cache.GetStringAsync("products:all");
        Console.WriteLine("Testing before cache invalidate --", cached);

        // Invalidate the all-products cache
        await _cache.RemoveAsync("products:all");
        Console.WriteLine(">>> Cache INVALIDATED: products:all after Create");

        cached = await _cache.GetStringAsync("products:all");
        Console.WriteLine("Testing after cache invalidate --", cached);
        return ToResponse(product);

    }

    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.Category = request.Category;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Invalidate both caches
        await _cache.RemoveAsync($"products:{id}");
        await _cache.RemoveAsync("products:all");
        Console.WriteLine($">>> Cache INVALIDATED: products:{id} and products:all after Update");

        return ToResponse(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return false;

        // Soft delete — just mark inactive
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync($"products:{id}");
        await _cache.RemoveAsync("products:all");
        Console.WriteLine($">>> Cache INVALIDATED: products:{id} and products:all after Delete");
        return true;
    }

    // Reduce stock when an order is placed (Order Service will call this)
    public async Task<bool> ReduceStockAsync(int productId, int quantity)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product is null || product.StockQuantity < quantity) return false;

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync($"products:{productId}");
        await _cache.RemoveAsync("products:all");

        return true;
    }


    private static ProductResponse ToResponse(Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category, p.IsActive, p.CreatedAt
    );
}
