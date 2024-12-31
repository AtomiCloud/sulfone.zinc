{
  inputs = {
    # util
    flake-utils.url = "github:numtide/flake-utils";
    treefmt-nix.url = "github:numtide/treefmt-nix";
    pre-commit-hooks.url = "github:cachix/pre-commit-hooks.nix";

    # registry
    nixpkgs.url = "nixpkgs/78058d810644f5ed276804ce7ea9e82d92bee293";
    nixpkgs-2305.url = "nixpkgs/nixos-23.05";
    nixpkgs-2411.url = "nixpkgs/nixos-24.11";
    nixpkgs-feb-23-24.url = "nixpkgs/0e74ca98a74bc7270d28838369593635a5db3260";
    atomipkgs.url = "github:kirinnee/test-nix-repo/v28.0.0";

  };
  outputs =
    { self

      # utils
    , flake-utils
    , treefmt-nix
    , pre-commit-hooks

      # registries
    , atomipkgs
    , nixpkgs
    , nixpkgs-2305
    , nixpkgs-2411
    , nixpkgs-feb-23-24

    } @inputs:
    (flake-utils.lib.eachDefaultSystem
      (
        system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
          pkgs-2305 = nixpkgs-2305.legacyPackages.${system};
          pkgs-2411 = nixpkgs-2411.legacyPackages.${system};
          pkgs-feb-23-24 = nixpkgs-feb-23-24.legacyPackages.${system};
          atomi = atomipkgs.packages.${system};
          pre-commit-lib = pre-commit-hooks.lib.${system};
        in
        with rec {
          pre-commit = import ./nix/pre-commit.nix {
            inherit packages pre-commit-lib formatter;
          };
          formatter = import ./nix/fmt.nix {
            inherit treefmt-nix pkgs;
          };
          packages = import ./nix/packages.nix
            {
              inherit pkgs pkgs-2305 pkgs-2411 atomi pkgs-feb-23-24;
            };
          env = import ./nix/env.nix {
            inherit pkgs packages;
          };
          devShells = import ./nix/shells.nix {
            inherit pkgs env packages;
            shellHook = checks.pre-commit-check.shellHook;
          };
          checks = {
            pre-commit-check = pre-commit;
            format = formatter;
          };
        };
        {
          inherit checks formatter packages devShells;
        }
      )
    )
  ;

}
