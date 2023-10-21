{ pkgs, pkgs-2305, atomi, atomi_classic, pkgs-oct-21-23 }:
let

  all = {
    atomipkgs_classic = (
      with atomi_classic;
      {
        inherit
          sg;
      }
    );
    atomipkgs = (
      with atomi;
      {
        inherit
          infisical
          mirrord
          pls;
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
    oct-21-23 = (
      with pkgs-oct-21-23;
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
atomipkgs_classic //
oct-21-23
