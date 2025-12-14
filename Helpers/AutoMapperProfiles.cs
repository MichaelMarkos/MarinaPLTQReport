using AutoMapper;
using maria.Dto;
using maria.Migrations;
using maria.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace maria.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {



            CreateMap<Report, ReportEditDto>().ReverseMap();

           

        }
    }
}
