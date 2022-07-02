using AutoMapper;
using ExpMedia.Application.ActivitiyFolder;
using ExpMedia.Application.ActivityAttendeeFolder;
using ExpMedia.Application.CommentFolder;
using ExpMedia.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<ActivityDTO, Activity>().ReverseMap();
            CreateMap<ActivityCreationDTO, Activity>().ReverseMap();
            CreateMap<ActivityView, Activity>().ReverseMap();

            CreateMap<CommentCreationDTO, Comment>().ReverseMap();


            CreateMap<ActivityAttendeeDTO, ActivityAttendee>().ReverseMap();
            CreateMap<ActivityAttendeeCreationDTO, ActivityAttendee>().ReverseMap();

        }
    }
}
