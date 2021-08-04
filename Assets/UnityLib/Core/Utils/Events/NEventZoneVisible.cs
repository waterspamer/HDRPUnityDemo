namespace Nettle {

    public class NEventZoneVisible : NEvent {

	public VisibilityZoneViewer manager;
	public VisibilityZone[] zones;

	protected override bool Get(){
		var activeZone = manager.ActiveZone;
		if(manager != null && activeZone != null && zones != null){
			foreach(var zone in zones){
				if(zone.name == activeZone.name){
					return true;
				}
			}
		}
		return false;
	}
}
}
