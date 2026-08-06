{
  pkgs,
  config,
  ...
}: {
  packages = with pkgs; [
    git
    jujutsu
    jq
    dotnet-ef
  ];

  languages.dotnet = {
    enable = true;
    package = pkgs.dotnetCorePackages.sdk_10_0;
  };

  env.PAYDAY_DB = let
    inherit (config.services.postgres) listen_addresses port;
    database = "payday";
    username = "kawid";
    password = "caue0914";
  in "Host=${listen_addresses};Port=${toString port};Database=${database};Username=${username};Password=${password}";

  services.postgres = {
    enable = true;
    listen_addresses = "127.0.0.1";
    port = 5432;

    initialDatabases = [
      {
        name = "payday";
      }
    ];

    initialScript =
      # sql
      ''
        CREATE USER kawid WITH PASSWORD 'caue0914';
        GRANT ALL PRIVILEGES ON DATABASE payday TO kawid;
      '';
  };
}
