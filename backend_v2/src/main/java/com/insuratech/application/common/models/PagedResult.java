package com.insuratech.application.common.models;

import java.util.List;

public record PagedResult<T>(
    List<T> items,
    int totalCount,
    int page,
    int pageSize
) {
    public int totalPages() {
        return (int) Math.ceil((double) totalCount / pageSize);
    }
    public boolean hasNextPage()     { return page < totalPages(); }
    public boolean hasPreviousPage() { return page > 1; }
}
