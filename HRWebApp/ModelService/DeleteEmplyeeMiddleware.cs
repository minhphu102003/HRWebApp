namespace HRWebApp.ModelService
{
    public class DeleteEmployeeMiddleware
    {
        public decimal employeeId { get; set; }
        public string id { get; set;}
        public override string ToString()
        {
            return $"Employee ID: {employeeId}, ID: {id}";
        }
    }

}