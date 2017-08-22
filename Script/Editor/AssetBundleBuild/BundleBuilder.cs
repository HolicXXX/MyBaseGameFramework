using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine.Windows;
using LitJson;

public class BundleBuilder : EditorWindow {

	private static readonly string _buildConfigFilePath = "Assets/BuildConfigs.xml";

	[MenuItem ("Custom Menu/Build Config File")]
	static private void BuildConfigFiles(){

		if (File.Exists (_buildConfigFilePath)) {
			File.Delete (_buildConfigFilePath);
		}
		AssetDatabase.RemoveUnusedAssetBundleNames ();

		XmlTextWriter w = new XmlTextWriter (_buildConfigFilePath, System.Text.Encoding.UTF8);
		w.Formatting = Formatting.Indented;
		w.WriteStartDocument ();
		w.WriteStartElement ("AssetBundleConfig");

		List<AssetInfo> assetList = new List<AssetInfo> ();
		Dictionary<string,HashSet<string>> variantDict = new Dictionary<string, HashSet<string>> ();

		string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames ();

		for (int i = 0; i < allAssetBundleNames.Length; ++i) {
			string bundleName = allAssetBundleNames [i];
			string variantName = null;
			string[] split = bundleName.Split (new char[]{ '.' });
			bundleName = split [0];
			variantName = split.Length > 1 ? split [1] : null;
			HashSet<string> variantList = null;
			if (!variantDict.TryGetValue (bundleName, out variantList)) {
				variantList = new HashSet<string> ();
				variantDict.Add (bundleName, variantList);
			}
			string[] allAssets = AssetDatabase.GetAssetPathsFromAssetBundle (allAssetBundleNames [i]);
			for (int j = 0; j < allAssets.Length; ++j) {
				if (!string.IsNullOrEmpty (variantName)) {
					variantList.Add (variantName);
				}
				AssetInfo info = new AssetInfo (AssetDatabase.AssetPathToGUID (allAssets [j]), allAssets [j], bundleName, variantName);
				assetList.Add (info);
			}
		}

		w.WriteStartElement ("AssetBundles");

		foreach (var pair in variantDict) {
			HashSet<string> bvarinats = pair.Value;
			if (bvarinats.Count > 0) {
				foreach (var v in bvarinats) {
					w.WriteStartElement ("AssetBundle");
					w.WriteAttributeString ("Name", pair.Key);
					w.WriteAttributeString("Variant", v);
					w.WriteEndElement ();
				}
			} else {
				w.WriteStartElement ("AssetBundle");
				w.WriteAttributeString ("Name", pair.Key);
				w.WriteEndElement ();
			}
		}

		w.WriteEndElement ();

		w.WriteStartElement ("Assets");
		for (int j = 0; j < assetList.Count; ++j) {
			w.WriteStartElement ("Asset");

			w.WriteAttributeString ("GUID", assetList [j].GUID);
			w.WriteAttributeString ("AssetFullPath", assetList [j].AssetName);
			w.WriteAttributeString ("AssetBundleName", assetList [j].BundleName);

			string variant = assetList [j].Variant;
			if (!string.IsNullOrEmpty (variant)) {
				w.WriteAttributeString ("Variant", variant);
			}

			w.WriteEndElement ();
		}
		w.WriteEndElement ();
		w.WriteEndElement ();
		w.WriteEndDocument ();
		w.Flush ();
		w.Close ();

	}

	[MenuItem ("Custom Menu/Build AssetBundle",true)]
	static private bool CheckConfigFile() {
		return File.Exists (_buildConfigFilePath);
	}
	[MenuItem("Custom Menu/Build AssetBundle")]
	static private void BuildAssetBundle(){
		BundleBuilder bb = EditorWindow.GetWindow<BundleBuilder>("Build Config File");
		bb.position = new Rect (new Vector2 (200f, 100f), new Vector2 (350f, 200f));
		bb.minSize = new Vector2 (350f, 200f);
	}

	static private BuildTarget GetBuildTarget() {
		BuildTarget target;
		#if UNITY_IPHONE
		target = BuildTarget.iOS;
		_targetIPhone = true;
		#elif UNITY_ANDROID
		target = BuildTarget.Android;
		_targetAndroid = true;
		#elif UNITY_STANDALONE_OSX
		target = BuildTarget.StandaloneOSXUniversal;
		_targetOSX = true;
		#elif UNITY_STANDALONE
		target = BuildTarget.StandaloneWindows;
		_targetWin = true;
		#else
		tartet = BuildTarget.NoTarget;
		#endif
		return target;
	}

	private static bool _targetIPhone = false;
	private static bool _targetAndroid = false;
	private static bool _targetOSX = false;
	private static bool _targetWin = false;
	
	private List<BuildABTargetInfo> _buildInfos;

	void OnEnable(){
		_targetIPhone = _targetAndroid = _targetOSX = _targetWin = false;
		GetBuildTarget ();
		_buildInfos = new List<BuildABTargetInfo> ();
	}
	
	void OnGUI(){
		GUILayout.BeginHorizontal ();
		{
			GUILayout.BeginVertical ();
			{
				_targetIPhone = GUILayout.Toggle (_targetIPhone, "TargetIPhone");
				_targetAndroid = GUILayout.Toggle (_targetAndroid, "TargetAndroid");
				_targetOSX = GUILayout.Toggle (_targetOSX, "TargetOSX");
				_targetWin = GUILayout.Toggle (_targetWin, "TargetWindows");
			}
			GUILayout.EndVertical ();
			GUILayout.BeginVertical ();
			{
				if (GUILayout.Button ("Build AssetBundles",GUILayout.Width(200f),GUILayout.Height(50f))) {
					var targets = GetBuildTargets ();
					_buildInfos.Clear ();
					XmlDocument config = new XmlDocument ();
					config.Load (_buildConfigFilePath);
					var builds = ParseBundleInfos (config);
					for (int i = 0; i < targets.Length; ++i) {
						var info = new BuildABTargetInfo (targets [i]);
						info.Manifest = BuildBundleForTarget (info, builds);
						_buildInfos.Add (info);
						if (CheckDependencyLoop (info.Manifest)) {
							Debug.LogError ("AssetBundle Dependencies is in Loop, Please Check!");	
						}
					}
				}
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal ();
	}
	
	private BuildTarget[] GetBuildTargets(){
		List<BuildTarget> ret = new List<BuildTarget> ();
		if (_targetIPhone) {
			ret.Add (BuildTarget.iOS);
		}
		if (_targetAndroid) {
			ret.Add (BuildTarget.Android);
		}
		if (_targetOSX) {
			ret.Add (BuildTarget.StandaloneOSXUniversal);
		}
		if (_targetWin) {
			ret.Add (BuildTarget.StandaloneWindows);
		}
		return ret.ToArray();
	}
	
	private AssetBundleBuild[] ParseBundleInfos(XmlDocument config){
		Dictionary<string, AssetBundleInfo> infos = new Dictionary<string, AssetBundleInfo> ();
		XmlNodeList bundles = config.SelectSingleNode ("AssetBundleConfig").SelectSingleNode("AssetBundles").SelectNodes ("AssetBundle");
		for (int i = 0; i < bundles.Count; ++i) {
			XmlNode node = bundles.Item (i);
			string name = node.Attributes.GetNamedItem ("Name").Value;
			if (!infos.ContainsKey (name)) {
				infos.Add (name, new AssetBundleInfo (name));
			}
		}
		XmlNodeList assets = config.SelectSingleNode ("AssetBundleConfig").SelectSingleNode("Assets").SelectNodes ("Asset");
		for (int i = 0; i < assets.Count; ++i) {
			XmlNode node = assets.Item (i);
			var attributes = node.Attributes;
			string guid = attributes.GetNamedItem ("GUID").Value;
			string name = attributes.GetNamedItem ("AssetFullPath").Value;
			string bundlename = attributes.GetNamedItem ("AssetBundleName").Value;
			XmlNode v = attributes.GetNamedItem ("Variant");
			string variant = v.IsNull () ? null : v.Value;
			infos [bundlename].AddAssetInfo (new AssetInfo (guid, name, bundlename, variant));
		}
		List<AssetBundleBuild> ret = new List<AssetBundleBuild> ();
		AssetBundleBuild configBuild = new AssetBundleBuild ();
		configBuild.assetBundleName = "Config";
		configBuild.assetNames = new string[1]{ "Assets/BuildConfigs.xml" };
		ret.Add (configBuild);
		foreach (var pair in infos) {
			ret.AddRange (pair.Value.GetBuilds ());
		}

		return ret.ToArray ();
	}

	private AssetBundleManifest BuildBundleForTarget(BuildABTargetInfo info, AssetBundleBuild[] builds){
		return BuildPipeline.BuildAssetBundles (info.BuildPath, builds, BuildAssetBundleOptions.ChunkBasedCompression, info.Target);
	}
	
	private bool CheckDependencyLoop(AssetBundleManifest manifest){
		return false;
	}
	
	private class AssetBundleInfo{
		public string BundleName{ get; set; }
		private List<AssetInfo> _assetInfos;
		public AssetBundleInfo(string bname){
			_assetInfos = new List<AssetInfo>();
			BundleName = bname;
		}
		public void AddAssetInfo(AssetInfo info){
			_assetInfos.Add (info);
		}

		public AssetBundleBuild[] GetBuilds(){
			Dictionary<string, List<string>> variantDict = new Dictionary<string, List<string>> ();
			List<string> normalAssets = new List<string> ();
			_assetInfos.ForEach (info => {
				if(!info.Variant.IsNull()){
					List<string> list = null;
					if(!variantDict.TryGetValue(info.Variant, out list)){
						list = new List<string>();
						variantDict.Add(info.Variant,list);
					}
					list.Add(info.AssetName);
				}else{
					normalAssets.Add(info.AssetName);
				}
			});
			List<AssetBundleBuild> ret = new List<AssetBundleBuild> ();
			AssetBundleBuild normal = new AssetBundleBuild ();
			normal.assetBundleName = BundleName;
			normal.assetNames = normalAssets.ToArray ();
			ret.Add (normal);
			foreach (var pair in variantDict) {
				AssetBundleBuild build = new AssetBundleBuild ();
				build.assetBundleName = BundleName;
				build.assetBundleVariant = pair.Key;
				build.assetNames = pair.Value.ToArray ();
				ret.Add (build);
			}
			return ret.ToArray ();
		}
	}
	private class AssetInfo{
		public string GUID{ get; set; }
		public string AssetName{ get; set; }
		public string BundleName{ get; set; }
		public string Variant{ get; set; }
		public AssetInfo(string guid,string name,string bundleName,string variant = null){
			GUID = guid;
			AssetName = name;
			BundleName = bundleName;
			Variant = variant;
		}
	}
}