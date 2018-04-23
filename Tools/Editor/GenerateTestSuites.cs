using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using GraphicsTestFramework;
using UnityEngine.Experimental.Rendering;

public class GenerateTestSuites
{
	[MenuItem("UTF/Generate HDRP TestSuites")]
	private static void GenerateSRPTestSuites()
	{
		// ---------- HDRP ----------
		string[] scenesGUIDs = AssetDatabase.FindAssets("t:Scene", new string[]{hdrp_TestsFolder});
		//Debug.Log("Found "+scenesGUIDs.Length+" scene(s) in "+hdrp_TestsFolder);

		// Find the different RP assets sub folders
		string[] subFolders = AssetDatabase.GetSubFolders(hdrp_PipelinesFolder);
		//Debug.Log("Found "+subFolders.Length+" HDRP assets folders in "+hdrp_PipelinesFolder+" .");

		foreach (string subFolder in subFolders)
		{
			string subFolderName = Path.GetFileName(subFolder);

			string[] hdrp_PipelineAssetsGUIDs = AssetDatabase.FindAssets("t:RenderPipelineAsset", new string[]{subFolder});

			if (hdrp_PipelineAssetsGUIDs.Length == 0) continue;

			var hdrp_PipelineAssets = new UnityEngine.Experimental.Rendering.HDPipeline.HDRenderPipelineAsset[ hdrp_PipelineAssetsGUIDs.Length ];

			for ( int i=0 ; i<hdrp_PipelineAssets.Length ; ++i)
			{
				hdrp_PipelineAssets[i] = AssetDatabase.LoadAssetAtPath<UnityEngine.Experimental.Rendering.HDPipeline.HDRenderPipelineAsset>( AssetDatabase.GUIDToAssetPath(hdrp_PipelineAssetsGUIDs[i]));
				//Debug.Log("Found RP asset: "+hdrp_PipelineAssets[i]);
			}

			GenerateTestSuiteFromScenes( hdrp_SuitesFolder , "HDRP_"+subFolderName, scenesGUIDs, hdrp_PipelineAssets);
		}

	}

	private static int hdrp_Platforms =
		1 << (int) RuntimePlatform.WindowsEditor |
		1 << (int) RuntimePlatform.WindowsPlayer |
		1 << (int) RuntimePlatform.OSXEditor |
		1 << (int) RuntimePlatform.OSXPlayer |
		1 << (int) RuntimePlatform.LinuxEditor |
		1 << (int) RuntimePlatform.LinuxPlayer |
		1 << (int) RuntimePlatform.WSAPlayerX86 |
		1 << (int) RuntimePlatform.WSAPlayerX64 |
		1 << (int) RuntimePlatform.PS4 |
		1 << (int) RuntimePlatform.XboxOne ;
	
	public static readonly string srp_RootPath = Path.GetDirectoryName( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets("SRPMARKER")[0] ) );
	public static readonly string utfCore_RootPath = Path.GetDirectoryName( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets("UTFCOREMARKER")[0] ) );
	public static readonly string hdrpSuites_RootPath = Path.GetDirectoryName( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets("HDRPSUITESMARKER")[0] ) );
	public static readonly string hdrpTests_RootPath = Path.GetDirectoryName( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets("HDRPTESTSMARKER")[0] ) );

	public static string CombinePath( string origin , params string[] foldersPath )
	{
		return foldersPath.Aggregate( origin, Path.Combine );
	}

	public static readonly string hdrp_TestsFolder = CombinePath( hdrpTests_RootPath, "Scenes" );
	public static readonly string hdrp_PipelinesFolder = CombinePath( hdrpTests_RootPath, "Common", "RP_Assets" );
	public static readonly string hdrp_SuitesFolder = CombinePath( hdrpSuites_RootPath , "Resources" );

	public static void GenerateTestSuiteFromScenes( string path, string name, string[] scenesGUIDs, RenderPipelineAsset[] renderPipelineAssets = null, int platforms = -1 )
	{
		Suite suite = new Suite();
		suite.suiteName = name;
		string suitePath = Path.Combine(path, name+".asset");

		if (renderPipelineAssets != null)
		{
			if (renderPipelineAssets.Length > 0)
				suite.defaultRenderPipeline = renderPipelineAssets[0];
			
			if (renderPipelineAssets.Length > 1)
			{
				AlternateSettings[] alternateSettings = new AlternateSettings[renderPipelineAssets.Length-1];

				for (int i=1 ; i<renderPipelineAssets.Length ; ++i)
				{
					alternateSettings[i-1] = new AlternateSettings(){
						renderPipeline = renderPipelineAssets[i],
						testSettings = null
					};
				}

				suite.alternateSettings = alternateSettings;
			}
		}
		
		Dictionary<string, Group> groups = new Dictionary<string, Group>();

		foreach ( string guid in scenesGUIDs)
		{
			string scenePath = AssetDatabase.GUIDToAssetPath(guid);
			SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>( scenePath );

			//Debug.Log(scene.name);

			string testName = Path.GetFileName( scenePath );
			string groupName = Path.GetFileName( Path.GetDirectoryName( scenePath ) );

			var regEx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9]"); // Regex to remove non alphanumerical character from the test and group name

			testName = regEx.Replace( testName, "");
			groupName = regEx.Replace( groupName, "");

			if (!groups.ContainsKey(groupName))
			{
				groups.Add(groupName, new Group());
				groups[groupName].tests = new List<Test>();
				groups[groupName].groupName = groupName;
			}

			Test test = new Test();
			test.name = testName;
			test.platforms = platforms;
			test.scene = scene;
			test.scenePath = scenePath;
			test.minimumUnityVersion = 5;	// See Common.cs unityVersionList	5 = 2018.2
			test.testTypes = 2; 			// 2 = frame comparison
			test.run = true;

			groups[groupName].tests.Add(test);
		}

		suite.groups = groups.Values.ToList();

		Suite oldsuite = AssetDatabase.LoadAssetAtPath<Suite>(suitePath);

		if (oldsuite != null)
		{
			EditorUtility.CopySerialized(suite, oldsuite);
			AssetDatabase.Refresh();
		}
		else
		{
			AssetDatabase.CreateAsset(suite, suitePath);
		}
	}
}