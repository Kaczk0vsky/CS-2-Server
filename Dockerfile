FROM cm2network/steamcmd:latest

USER root
WORKDIR /home/steam

COPY start.sh /home/steam/start.sh
RUN chmod +x /home/steam/start.sh

USER steam

ENTRYPOINT ["/home/steam/start.sh"]