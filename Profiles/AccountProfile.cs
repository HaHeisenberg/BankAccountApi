using AutoMapper;
using BankProject.Shared;
namespace BankAccountApi.Profiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Entities.AccountEntity, AccountDto>();
            CreateMap<Models.AccountForCreationDto, Entities.AccountEntity>();
            CreateMap<Entities.AccountEntity, Models.AccountForUpdateDto>();
            CreateMap<Models.AccountForUpdateDto, Entities.AccountEntity>();
        }
    }
}
