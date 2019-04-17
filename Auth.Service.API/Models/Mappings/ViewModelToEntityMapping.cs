using Auth.Service.API.Entities;
using Auth.Service.API.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Service.API.Models.Mappings
{
    public class ViewModelToEntityMapping : Profile
    {
        public ViewModelToEntityMapping()
        {
            CreateMap<RegistrationViewModel, User>()
                .ForMember(au => au.UserName, map => map.MapFrom(vm => vm.Email))
                .ReverseMap();
        }
    }
}
