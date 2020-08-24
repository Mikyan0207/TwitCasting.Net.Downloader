[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]


<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/mikyan0207/TwitCasting.Net.Downloader">
    <img src="images/logo.png" alt="Logo" width="120" height="120">
  </a>

  <h3 align="center">TwitCasting Downloader</h3>

  <p align="center">
    Record or Download any TwitCasting live
    <br />
    <a href="https://github.com/mikyan0207/TwitCasting.Net.Downloader/issues">Report Bug</a>
    ·
    <a href="https://github.com/mikyan0207/TwitCasting.Net.Downloader/issues">Request Feature</a>
  </p>
</p>



<!-- TABLE OF CONTENTS -->
## Table of Contents

* [About the Project](#about-the-project)
  * [Built With](#built-with)
* [Getting Started](#getting-started)
  * [Prerequisites](#prerequisites)
  * [Installation](#installation)
* [Usage](#usage)
* [Roadmap](#roadmap)
* [Contributing](#contributing)
* [License](#license)
* [Contact](#contact)
* [Acknowledgements](#acknowledgements)



<!-- ABOUT THE PROJECT -->
## About The Project

TwitCasting.Net.Downloader is a tool to download or record any TwitCasting livestream.<br>
This project does not use the official TwitCasting API as it does not provide any tools to record or download a livestream.<br>
If you want to interact with the TwitCasting API you can use [TwitCasting.Net](https://github.com/Mikyan0207/TwitCasting.Net).


### Built With

* [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)



<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

FFmpeg is required to run TwitCasting.Net.Downloader. You can download it from the official website or use the executable available with TwitCasting.Net.Downloader.

### Installation
 
1. [Go to release page](https://github.com/Mikyan0207/TwitCasting.Net.Downloader/releases)

2. Download the version corresponding to your platform (Windows / Linux)

### Usage

- Download a live

Replace `{user}` with the TwitCaster's username.<br>
Replace `{id}` with the id of the video you want to download.

```sh
> ./tc-downloader -u {user} -l {id}
```


- Record a live in real-time

```sh
> ./tc-downloader -u {user}
```


- Specify output file

```sh
> ./tc-downloader -u {user} -l {id} -o {filename}
```

<!-- ROADMAP -->
## Roadmap

See the [open issues](https://github.com/mikyan0207/TwitCasting.Net.Downloader/issues) for a list of proposed features (and known issues).



<!-- CONTRIBUTING -->
## Contributing

Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request



<!-- LICENSE -->
## License

Distributed under the [MIT License][license-url]. See `LICENSE` for more information.



<!-- CONTACT -->
## Contact

Mikyan0207 - [@mikyan0207](https://twitter.com/mikyan0207)

Project Link: [https://github.com/mikyan0207/TwitCasting.Net.Downloader](https://github.com/mikyan0207/TwitCasting.Net.Downloader)



<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements

* [CommandLineParser](https://github.com/commandlineparser/commandline)
* [DSharpPlus (AsyncEvents)](https://github.com/DSharpPlus/DSharpPlus)
* [FFmpeg](https://ffmpeg.org/)
* [Xabe.FFmpeg](https://github.com/tomaszzmuda/Xabe.FFmpeg)


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/mikyan0207/TwitCasting.Net.Downloader.svg?style=flat-square
[contributors-url]: https://github.com/mikyan0207/TwitCasting.Net.Downloader/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/mikyan0207/TwitCasting.Net.Downloader.svg?style=flat-square
[forks-url]: https://github.com/mikyan0207/TwitCasting.Net.Downloader/network/members
[stars-shield]: https://img.shields.io/github/stars/mikyan0207/TwitCasting.Net.Downloader.svg?style=flat-square
[stars-url]: https://github.com/mikyan0207/TwitCasting.Net.Downloader/stargazers
[issues-shield]: https://img.shields.io/github/issues/mikyan0207/TwitCasting.Net.Downloader.svg?style=flat-square
[issues-url]: https://github.com/mikyan0207/TwitCasting.Net.Downloader/issues
[license-shield]: https://img.shields.io/github/license/mikyan0207/TwitCasting.Net.Downloader.svg?style=flat-square
[license-url]: https://github.com/mikyan0207/TwitCasting.Net.Downloader/blob/master/LICENSE.txt
[product-screenshot]: images/logo.png
