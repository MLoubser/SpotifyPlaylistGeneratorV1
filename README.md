# SpotifyPlaylistGeneratorV1
## Purpose / FAQ
I started this project to explore more advanced features of c# as well as play around with the YouTube and Spotify API's. Some components in this project:
- Microsoft EntityFramework with Identity for Authentication/Authorization
- String encryption service: Used to encrypt and decrypt API / Refresh tokens stored in the Database 
- YouTube API integration - more on this later
- Spotify API integration - more on this later
- Hosted service as a background worker

### Ok, but what does this app actually do?
To put it short: this app creates playlists in Spotify from YouTube videos, those 3 hour long videos (usually some EDM/House/80s mix-tape :D)

### Ok, but how do you do that?
Simple, by following these steps:
1. The user inputs an video id (currently this is hardcoded in the home controller)
2. Using Google's YouTube V3 library, query the id and fetch the description in the video
3. Take the description and parse it into a list of potential songs
4. Using the Spotify API (standard HTTP requests - no library needed if you like making life difficult), iterate through the track list and query for the Spotify id of each individual track. Save this in a new list.
5. Finally, create an playlist and add all of the id's from the previous step.

### Ok, but how do you handle all these API requests without timing out the client's HTTP request?
This is where the magic of .NET Core hosted services come into play. Using a channel, I pass on an internal request with relevant info to a hosted service doing all of the heavy lifting. The (thread safe) channel ensures that playlist requests can be processed in the background without slowing the main web server thread(s).

### Ok, but why not just use a message broker like RabbitMQ?
Well, if this thing was feasible in real life, then a message broker would be ideal. Realistically, this is *only a proof of concept (at best)* - for educational purposes. I wanted to experiment with various options for multithreading and keep the project as easy as possible to run (without external components).

### Ok, but why encrypt the API and refresh tokens if this is just an educational exercise?
To be honest, this was just my curiosity taking over. I like cryptography and the mechanics behind AES encryption were to tempting.

### Ok, but why..?
Why not...

## How it works (but with pictures)
### Prereqs
You need to configure this system to work with your own API keys - use the `appsettingsExample.json` as an template to fill in, using the links to create your own projects in YouTube/Spotify. Save this new config file as `appsettings.json` in the same directory as the example file.

Currently the system is configured to create a playlist on the home controller - lazy, I know. Refer to the home controller (`Controllers/HomeController.cs`) and modify the YoutubeUrl and PlaylistName in the PlaylistRequestItems as required. I plan on building dedicated screens for this in the future, letting the user do the input.

### Register / Login
The end user can use the register and login screens to create an account in the app and reach the dashboard (home). 

### Log in to Spotify
When you sign in the first time you need to give this app permission from Spotify to create playlists. 

![Dashboard_WithoutSFSignIn](https://github.com/Slakkiii/SpotifyPlaylistGeneratorV1/assets/108271978/ae1b797b-f931-4ddc-9889-b6aff65435c0)

On the dashboard you will see a link to "Log in with spotify", this will redirect you to Spotify. 

![SpotifyAuthScreen](https://github.com/Slakkiii/SpotifyPlaylistGeneratorV1/assets/108271978/ab0b222c-b215-443c-a1c5-28cb63c0bae3)

You will be redirected to the home page after giving authorization from Spotify. Note the text now says you are logged into spotify.

### Create Playlist
Every time you reach the home controller it will (attempt) to create the playlist as configured above. You should be able to see the playlist(s) created in spotify:

![SpotifyPlaylistGenerated](https://github.com/Slakkiii/SpotifyPlaylistGeneratorV1/assets/108271978/65b45c46-1ef5-43f6-a9bc-1bd33b2b33fc)

## TODO Items:
- [x] Implement core functionality as proof of concept
- [ ] UI Overhaul - Rework the interface so that users can paste in an YouTube URL and click "Go"
- [ ] Add support for other formats of track items in the video description (currently only supports one)
- [ ] Add support (maybe) for other sources of music items (like SoundCloud, Deezer etc.)
- [ ] Give users more feedback regarding status of playlist generation
- [ ] Give users ability to change / remove song titles before playlist is generated
- [ ] Refine Spotify search - parse the artist and song title separately and include them separate in the search query
