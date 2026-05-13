namespace OrderService.DTOs;

public record OrderItemRequest
(
    int ProductId,
    int Quantity
);
public record CreateOrderRequest(
    List<OrderItemRequest> Items
);

public record OrderItemResponse(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record OrderResponse(
    int Id,
    int UserId, 
    string Status,
    decimal TotalAmount,
    List<OrderItemResponse> Items,
    DateTime CreatedAt
);

// This is what we get back from Product Service
public record ProductDto(
    int Id,
    string Name,
    decimal Price,
    int StockQuantity,
    bool IsActive
);