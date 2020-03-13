[![Paypal](https://img.shields.io/badge/Paypal-Donate-success?style=for-the-badge&logo=paypal)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=ELFGQ65FJFPVQ&currency_code=CAD&source=url) 
[![Discord](https://img.shields.io/discord/674782527139086350?color=7289DA&label=Discord&style=for-the-badge)](https://discord.gg/ATCM64M)
[![DockerHub](https://img.shields.io/badge/Docker-Hub-%23099cec?style=for-the-badge&logo=docker)](https://hub.docker.com/r/darkalfx/requestrr)
[![DockerHub](https://img.shields.io/badge/GitHub-Repo-lightgrey?style=for-the-badge&logo=github)](https://github.com/darkalfx/requestrr/)

Requestrr 
=================

![logo](https://i.imgur.com/0UzLYvw.png)


Installation & Configuration
==================

For Discord, make sure that your bot has the following permissions

    Read Messages
    Embed Links
    Read Message History
    Use External Emojis
    Send Messages
    Manage Messages
    Mention @everyone
    Add Reactions


Docker Setup & Start
==================

First pull the image from docker hub using docker push darkalfx/requestrr

Then use the following command create and start the container:

    docker run --name requestrr \
      -p 5060:5060 \
      -v path to config:/root/config \
      --restart=unless-stopped \
      darkalfx/requestrr

Then simply access the web portal at http://youraddress:5060/ to create your admin account, then you can configure everything through the web portal. 
