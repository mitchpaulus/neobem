FROM ubuntu:24.04

# Install dependencies
RUN apt-get update && apt-get install -y pandoc texlive-full unzip curl

# Install iosevka font
WORKDIR /usr/share/fonts
RUN curl -sL https://github.com/be5invis/Iosevka/releases/download/v33.0.0/PkgTTF-Iosevka-33.0.0.zip -o iosevka.zip && \
    unzip iosevka.zip && \
    fc-cache    
