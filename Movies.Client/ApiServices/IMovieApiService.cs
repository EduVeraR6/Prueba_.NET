using Movies.Client.Models;

namespace Movies.Client.ApiServices
{
    public interface IMovieApiService
    {

        Task<List<Movie>> GetMovies();
        Task<Movie> GetMovie(int id );
        Task<Movie> CreateMovie(MovieRegister movie);
        Task<Movie> UpdateMovie(int id ,Movie movie);
        Task DeleteMovie(int id);

        Task<UserInfoViewModel?> GetUserInfo();

    }
}
