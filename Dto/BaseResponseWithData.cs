using static System.Runtime.InteropServices.JavaScript.JSType;

namespace maria.Dto
{
    public class BaseResponseWithData<ViewModel>
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public ViewModel Data { get; set; }
    }
}
