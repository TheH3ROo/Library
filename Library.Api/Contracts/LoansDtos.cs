namespace Library.Api.Contracts
{
    public sealed class BorrowRequest
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public DateTime NowUtc { get; set; }
    }

    public sealed class ReturnRequest
    {
        public Guid LoanId { get; set; }
        public DateTime NowUtc { get; set; }
    }

    public sealed class LoanResponse
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public Guid UserId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
