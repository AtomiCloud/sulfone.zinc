{ pkgs, pkgs-2411, atomi }:
let

  all = {
    atomipkgs = (
      with atomi;
      {
        inherit
          infrautils
          atomiutils
          infralint
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
