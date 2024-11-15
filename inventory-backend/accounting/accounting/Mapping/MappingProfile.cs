using accounting.DTOs;
using accounting.Entities;
using AutoMapper;

namespace accounting.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductDTO, Product>();
            CreateMap<AccountDTO, Account>().ReverseMap();
            CreateMap<MasterTransactionDTO, MasterTransaction>().ReverseMap();
            CreateMap<TransactionDTO, Transaction>().ReverseMap();
        }
    }
}
