using AutoMapper;
using CourseLibrary.API.Dtos;
using CourseLibrary.API.Entities;

namespace CourseLibrary.API.Profiles
{
    public class CoursesProfile : Profile
    {
        public CoursesProfile()
        {
            CreateMap<Course, CourseDto>();
            CreateMap<CourseForCreationDto, Course>();
            CreateMap<CourseForUpdateDto, Course>();
            CreateMap<Course, CourseForUpdateDto>();
        }
    }
}