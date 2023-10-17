{ pkgs, pkgs-2305, atomi, atomi_classic, pkgs-sep-12-23 }:
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
          hadolint;
      }
    );
    sep-12-23 = (
      with pkgs-sep-12-23;
      {

        helm = kubernetes-helm;
        npm = nodePackages.npm;
        inherit
          coreutils
          yq-go
          gnused
          gnugrep
          bash
          jq
          findutils

          git

          dotnet-sdk_8

          # infra
          docker
          k3d
          tilt

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
sep-12-23
