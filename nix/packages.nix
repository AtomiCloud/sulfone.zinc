{ pkgs, pkgs-2505, pkgs-unstable, atomi }:
let

  all = rec {
    atomipkgs = (
      with atomi;
      rec {

        dotnetlint = atomi.dotnetlint.override { dotnetPackage = nix-2505.dotnet; };
        helmlint = atomi.helmlint.override { helmPackage = infrautils; };

        inherit
          infrautils
          atomiutils
          infralint
          pls
          sg;
      }
    );
    nix-unstable = (
      with pkgs-unstable;
      { }
    );
    nix-2505 = (
      with pkgs-2505;
      {
        dotnet = dotnet-sdk;
        inherit
          infisical
          git

          # linter
          treefmt
          gitlint
          shellcheck;
      }
    );
  };
in
with all;
nix-2505 //
nix-unstable //
atomipkgs
