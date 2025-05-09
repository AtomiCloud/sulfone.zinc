{ pkgs, packages }:
with packages;
{
  system = [
    atomiutils
  ];

  dev = [
    pls
    git
    openapi_to_postmanv2
  ];

  infra = [
    infrautils
  ];

  main = [
    dotnet
    infisical
  ];

  lint = [
    # core
    treefmt
    gitlint
    shellcheck
    infralint
    dotnetlint
    helmlint
    sg
  ];

  releaser = [
    sg
  ];
}
