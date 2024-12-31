{ pkgs, pkgs-2305, pkgs-2411, atomi, pkgs-feb-23-24 }:
let

  all = {
    atomipkgs = (
      with atomi;
      {
        inherit
          mirrord
          pls
          sg;
      }
    );
    nix-2305 = (
      with pkgs-2305;
      {
        inherit
          tilt
          dotnet-sdk_8
          hadolint;
      }
    );
    nix-2411 = (
      with pkgs-2411;
      {
        inherit
          infisical;
      }
    );
    feb-23-24 = (
      with pkgs-feb-23-24;
      {
        nodejs = nodejs_20;
        helm = kubernetes-helm;
        npm = nodePackages.npm;
        inherit
          doppler
          coreutils
          yq-go
          gnused
          gnugrep
          bash
          jq
          findutils

          git


          # infra
          docker
          k3d

          kubectl

          # linter
          treefmt
          gitlint
          shellcheck
          helm-docs
          ;
      }
    );
  };
in
with all;
nix-2411 //
nix-2305 //
atomipkgs //
feb-23-24
