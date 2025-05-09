{ pkgs, pkgs-2411, atomi }:
let

  all = rec {
    atomipkgs = (
      with atomi;
      rec {

        dotnetlint = atomi.dotnetlint.override { dotnetPackage = nix-2411.dotnet; };
        helmlint = atomi.helmlint.override { helmPackage = infrautils; };

        inherit
          infrautils
          atomiutils
          infralint
          openapi_to_postmanv2
          pls
          sg;
      }
    );
    nix-2411 = (
      with pkgs-2411;
      {
        dotnet = dotnet-sdk_8;
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
nix-2411 //
atomipkgs 
