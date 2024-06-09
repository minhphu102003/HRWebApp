namespace HRWebApp.ModelService
{
    public class EmployeeMiddleware
    {
        public decimal employeeId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool shareHolder { get; set; }
        public bool gender { get; set; }
        public string ethnicity { get; set; }
        public float paidToDate { get; set; }
        public float paidLastYear { get; set; }
        public string employmentStatus {  get; set; }
        public int vacationDays {  get; set; }
        public int benefitID { get; set; }
        public string idMongo { get; set; }

        public override string ToString()
        {
            return $"Employee ID: {employeeId}, " +
                   $"First Name: {firstName}, " +
                   $"Last Name: {lastName}, " +
                   $"Share Holder: {shareHolder}, " +
                   $"Gender: {gender}, " +
                   $"Ethnicity: {ethnicity}, " +
                   $"Paid To Date: {paidToDate}, " +
                   $"Paid Last Year: {paidLastYear}, " +
                   $"Employment Status: {employmentStatus}, " +
                   $"Vacation Days: {vacationDays}, " +
                   $"Benefit ID: {benefitID}, " +
                   $"ID Mongo: {idMongo}";
        }

    }


}