Requestrr [![Paypal](https://img.shields.io/liberapay/patrons/0?color=3b7bbf%20&label=Paypal&style=flat)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=ELFGQ65FJFPVQ&currency_code=CAD&source=url) [![Discord](https://img.shields.io/badge/Discord-Invite-%237289da)](https://discord.gg/ATCM64M)
=================

![logo](https://i.imgur.com/h04cmDz.png)


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
      -e PUID=1000 \
      -e PGID=1000 \
      -e TZ=America/New_York \
      -p 5060:5060
      -v path to config:/root/config \
      --restart=unless-stopped \
      darkalfx/requestrr

Then simply access the web portal at http://youraddress:5060/ to create your admin account, then you can configure everything through the web portal. 