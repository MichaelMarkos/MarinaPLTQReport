namespace maria.Model
{
    public class Elevator
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public long UserId { get; set; } = 1;
        public string? ReportNumber { get; set; }
        public string reportType { get; set; }
        public string? InvoiceNumber { get; set; }
        public string CompanyName { get; set; }
        public string salesName { get; set; }
        public string? ProjectAddress { get; set; }

        public int resizableSquarewidth { get; set; }
        public int resizableSquareHeight { get; set; }


        public string typeElevator { get; set; }
        public string shapeType { get; set; }
        public int? widthShape { get; set; }
        public int? heightShape { get; set; }
        public int? radiusShape { get; set; }
        // public int directionShape { get; set; }
        public int floors { get; set; }
        public int garagsNum { get; set; }
        public int foundationHeight { get; set; }
        public int? capinaHeight { get; set; }
        public int? liftWidth { get; set; }
        public int? rightWidth { get; set; }
        public int? centerWidth { get; set; }
        public int directionWidth { get; set; }
        public int directionHeight { get; set; }
        public string wellStatus { get; set; }
        public string capinaStatus { get; set; }
        public string floorHeights { get; set; }
        public string? garagsHeights { get; set; }
        public string doorDirections { get; set; }
        public string? garagsDirections { get; set; }
        public string? workRequied { get; set; }

        public string? doortwoDirections { get; set; }
        public string? garagstwoDirections { get; set; }
        public bool twoDirectionFlag { get; set; }

        public string? Notes { get; set; }
        public string? WallStatusForList { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public string? PdfFilePath { get; set; } 

        //  public string ClientName { get; set; }
        //public string ClientSignaturePath { get; set; }
        //public string TechSignaturePath { get; set; }
        public string TechName { get; set; }
        public string? PhoneNum { get; set; }


        public string WellImagePath { get; set; }
        public string DirectionImagePath { get; set; }
        public string ResizableImagePath { get; set; }



        public ICollection<ElevatorImage> elevatorFiles { get; set; } = new List<ElevatorImage>();
    }
}
