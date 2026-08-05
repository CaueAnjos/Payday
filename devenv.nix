{pkgs, ...}: {
  packages = with pkgs; [git jujutsu];

  languages.dotnet = {
    enable = true;
    package = pkgs.dotnetCorePackages.sdk_10_0;
  };

  services.postgres.enable = true;
}
