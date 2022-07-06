export interface ApiResponse<T> {
    Messages: string[],
    Result: T
}

export interface PaginatedList<T> {
    Total: number,
    Results: T[]
}
