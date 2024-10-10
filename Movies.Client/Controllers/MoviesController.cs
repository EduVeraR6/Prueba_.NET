using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Movies.Client.ApiServices;
using Movies.Client.Models;

namespace Movies.Client.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        private readonly IMovieApiService _movieApiService;

        public MoviesController(IMovieApiService movieApiService)
        {
            _movieApiService = movieApiService ?? throw new ArgumentNullException(nameof(movieApiService));
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            await LogTokenAndClaims();
            return View(await _movieApiService.GetMovies());
        }




        public async Task LogTokenAndClaims()
        {
            var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            
            var refreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
            
   
        }

        public async Task<IActionResult> OnlyAdmin()
        {
            var userInfo = await _movieApiService.GetUserInfo();
            return View(userInfo);
        }


 
        public async Task<IActionResult> Details(int? id)
        {
            Movie pelicula = await _movieApiService.GetMovie(id.Value);

            if(pelicula == null)
            {
                return RedirectToAction("Index");
            }

            return View(pelicula);
        }


        public  IActionResult Create()
        {
         
            return View(); 
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MovieRegister movie)
        {
            if (ModelState.IsValid)
            {
                 _movieApiService.CreateMovie(movie);

                return RedirectToAction("Index");
            }
            return View(movie);

        }


        public async Task<IActionResult> Edit(int? id)
        {
            Movie pelicula = await _movieApiService.GetMovie(id.Value);

            if (pelicula == null)
            {
                return RedirectToAction("Index");
            }
            return View(pelicula);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {
            if (ModelState.IsValid)
            {
                _movieApiService.UpdateMovie(id , movie);

                return RedirectToAction("Index");
            }
            return View(movie);
        }

      
        public async Task<IActionResult> Delete(int? id)
        {
            Movie pelicula = await _movieApiService.GetMovie(id.Value);

            if (pelicula == null)
            {
                return RedirectToAction("Index");
            }

            return View(pelicula);
           
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _movieApiService.DeleteMovie(id);
            
            return RedirectToAction("Index");
        }

        private bool MovieExists(int id)
        {
            return true;
        }

        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

    }
}
