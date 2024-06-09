namespace HRWebApp.ModelService
{
    public class EmployeeSip
    {
        public string _id { get; set; }
        public int employeeId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int vacationDays { get; set; }
        public int paidToDate { get; set; }
        public int paidLastYear { get; set; }
        public int payRate { get; set; }
        public int payRateId { get; set; }

        public override string ToString()
        {
            return $"EmployeeSip: {{ _id: {_id}, employeeId: {employeeId}, firstName: {firstName}, lastName: {lastName}, vacationDays: {vacationDays}, paidToDate: {paidToDate}, paidLastYear: {paidLastYear}, payRate: {payRate}, payRateId: {payRateId} }}";
        }
    }
}