using AutoMapper;

using Ex077.Entities.Options;
using Ex077.ViewModels;

namespace Ex077.Entities.Profiles;

public class HomeViewModelProfile : Profile
{
	public HomeViewModelProfile()
	{
		CreateMap<HomeOptions, HomeViewModel>();
	}
}
