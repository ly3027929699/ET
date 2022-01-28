namespace ET
{
    public static class PlayerComponentSystem
    {
        public static int CurrPlayerNum(this PlayerComponent self)
        {
            return self.Children.Count;
        }

        public static bool CanAddPlayer(this PlayerComponent self)
        {
            return self.CurrPlayerNum() < SteamComponent.MaxPlayer;
        }
        
        public static int Add(this PlayerComponent self,Session session)
        {
            var player=self.AddChild<Player>();
            session.AddComponent<SessionPlayerComponent,long>(player.Id);
            Log.Info($"------------------------{self.DomainScene().Name} add player : {player.Id}------------------------");
            return ErrorCode.ERR_Success;
        }

        public static void Remove(this PlayerComponent self, long id)
        {
            if (!self.Children.Remove(id))
            {
                Log.Error($"remove child fail where id is {id}");
            }
            Log.Info($"------------------------{self.DomainScene().Name} remove player : {id}------------------------");
        }
    }
}