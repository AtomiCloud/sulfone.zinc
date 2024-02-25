{ pkgs, pkgs-2305, atomi, pkgs-feb-23-24 }:
let

  all = {
    atomipkgs = (
      with atomi;
      {
        inherit
          infisical
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
nix-2305 //
atomipkgs //
feb-23-24
