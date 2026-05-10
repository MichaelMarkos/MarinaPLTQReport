namespace maria.Dto
{
    public class ProjectContractDto
    {
        public int ID { get; set; }
        public int ProjectID { get; set; }
        public string Name { get; set; }
        public string ProjectSerial { get; set; }
        public string ProductSerial { get; set; }
        public string ContractStartDate { get; set; }
        public string ContractEndDate { get; set; }
        public string ContractSerial { get; set; }
        public string ContractStatus { get; set; }
        public string ContractInfo { get; set; }
        public string Client { get; set; }
    }

    public class ApiResponse
    {
        public bool Result { get; set; }
        public List<string> Errors { get; set; }
        public List<ProjectContractDto> Data { get; set; }
    }
}
