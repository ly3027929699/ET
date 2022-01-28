
using UnityEngine.UI;

namespace ET
{
	public class UIJoinLobbyComponent : Entity, IAwake
	{
		public Button btnRefresh;
		public ScrollRect scrList;
		public SingleLobbyButton item_prefab;
		
		public SingleLobbyButton[] m_items;
		public bool isRequesting = false;
	}
}
