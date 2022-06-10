using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[System.Serializable]
public class CardDataBaseEditor : EditorWindow
{
	public static CardDataBaseData setting; //カードデータベース

	public static SerializedObject serializedObject;
	public static SerializedProperty serializedProperty;
	public static ReorderableList reorderable;

	static int selectObjectIndex = -1; //現在選択されているカード情報のインデックス
	static string createName = "";

	List<int> selectIndexList = new List<int>(); //インデックスの並び

	Vector2 objectFieldSlider = Vector2.zero;
	Vector2 inspectorFieldSlider = Vector2.zero;

	enum ViewSort
	{
		Trap_OnGround,
		Trap_Pass,
		Magic
	}
	ViewSort viewSort;

	CardData.Type cardType;
	CardData.CostType costType;

	static bool awake;

	/// <summary>
	/// UnityEditor上から開けるようにする
	/// </summary>
	[MenuItem("DataBase/CardDataEditor")]
	static void Open()
	{
		EditorWindow.GetWindow<CardDataBaseEditor>("CardDataBase");

		LoadAssets();

		awake = false;
	}

	/// <summary>
	/// 最初の読み込み
	/// </summary>
	private void OnEnable()
	{
		LoadAssets();
		ListGUI();
	}

	/// <summary>
	/// 色々なデータを読み込む
	/// </summary>
	static void LoadAssets()
	{
		var path = "Assets/CardDataBaseEditorData.asset";
		setting = AssetDatabase.LoadAssetAtPath<CardDataBaseData>(path);

		if (setting == null)
		{
			setting = ScriptableObject.CreateInstance<CardDataBaseData>();
			AssetDatabase.CreateAsset(setting, path);
		}
	}

	/// <summary>
	/// Listを並び替え可能なReOrderbleListに変換
	/// </summary>
	static void ListGUI()
	{
		serializedObject = new SerializedObject(setting);
		serializedProperty = serializedObject.FindProperty("CardList");

		if (reorderable == null)
		{
			reorderable = new ReorderableList(serializedObject, serializedProperty);

			// 並び替え可能か
			reorderable.draggable = true;

			// タイトル描画時のコールバック
			// 上書きしてEditorGUIを使えばタイトル部分を自由にレイアウトできる
			reorderable.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Title");

			// プロパティの高さを指定
			reorderable.elementHeightCallback = index => 30;

			// 要素の描画時のコールバック
			// 上書きしてEditorGUIを使えば自由にレイアウトできる
			reorderable.drawElementCallback = (rect, index, isActive, isFocused) =>
			{
				var elementProperty = serializedProperty.GetArrayElementAtIndex(index);
				rect.height = EditorGUIUtility.singleLineHeight;
				string str = (setting.CardList[index] == null) ? "Null Data" : setting.CardList[index].name;
				string tier = (setting.CardList[index] == null) ? "-" : setting.CardList[index].tier.ToString();
				EditorGUI.PropertyField(rect, elementProperty, new GUIContent(" [" + tier + "]  " + index.ToString("D3") + ": " + str));
			};

			// +ボタンが押された時のコールバック
			reorderable.onAddCallback = list =>
			{
				Debug.Log("+ clicked.");
				//setting.CardList.Add(new CardData());
				serializedProperty.arraySize++;
			};

			// -ボタンが押された時のコールバック
			reorderable.onRemoveCallback = list =>
			{
				Debug.Log("- clicked : " + list.index + ".");
				//setting.CardList.RemoveAt(list.index);
				serializedProperty.DeleteArrayElementAtIndex(list.index);
				selectObjectIndex = -1;
			};

			reorderable.onSelectCallback = list =>
			{
				selectObjectIndex = list.index;
			};

			reorderable.drawElementBackgroundCallback = OnDrawElementBackground;
		}
	}

	/// <summary>
	/// カードの種類ごとに背景の色を変える
	/// </summary>
	static void OnDrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
	{
		if (setting.CardList.Count - 1 < index) return;

		Texture2D tex = new Texture2D(1, 1);
		Color color = new Color(0, 0, 0, 1);
		if (setting == null) Debug.Log("おかしい"); //設定が存在しないのは致命的なバグ
		else if (setting.CardList[index] == null) color = new Color(0.5f, 0.5f, 0.5f, 1f); //なにも設定されていなければ灰色に
		else if (setting.CardList[index].type == CardData.Type.Magic) //タイプが魔法なら
		{
			if (setting.CardList[index].costType == CardData.CostType.Mana) color = new Color(0, 0, 0.5f, 1f); //コストがマナなら緑色に
			if (setting.CardList[index].costType == CardData.CostType.Cooldown) color = new Color(0.0f, 0.5f, 0.0f, 1f); //コストがクールダウンなら青色
		}
		else color = new Color(0.5f, 0.0f, 0, 1f); //それでもなければ赤色に

		//選択中なら色を濃くする
		if (isFocused)
		{
			Color temp = color;
			temp.r = temp.r > 0 ? 1 : 0;
			temp.g = temp.g > 0 ? 1 : 0;
			temp.b = temp.b > 0 ? 1 : 0;
			temp.a = temp.a > 0 ? 1 : 0;
			color = temp;
		}

		tex.SetPixel(0, 0, color);
		tex.Apply();
		GUI.DrawTexture(rect, tex as Texture);
	}

	/// <summary>
	/// 設定ファイルのFolder生成
	/// </summary>
	static void CreateFolder()
	{
		var path = "Assets/CardData/";
		if (!System.IO.Directory.Exists(path + "Magic"))
		{
			System.IO.Directory.CreateDirectory(path + "Magic");
		}
		if (!System.IO.Directory.Exists(path + "Trap/OnGround"))
		{
			System.IO.Directory.CreateDirectory(path + "Trap/OnGround");
		}
		if (!System.IO.Directory.Exists(path + "Trap/Pass"))
		{
			System.IO.Directory.CreateDirectory(path + "Trap/Pass");
		}
	}

	/// <summary>
	/// GUIの描画
	/// </summary>
	private void OnGUI()
	{
		//最初のソート
		if (awake == false)
		{
			awake = true;
			Sort();
		}

		serializedObject.Update();

		EditorGUI.BeginChangeCheck();

		//MenuGUI(上部)
		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.LabelField("カード情報の総数: " + setting.CardList.Count.ToString());
			EditorGUILayout.Space(10);
			MenuGUI();
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.Space(10);

		//ConfigGUI(下部)
		EditorGUILayout.BeginHorizontal();
		{
			//オブジェクト選択部分(左)
			EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(350));
			{
				ObjectFieldGUI();
			}
			EditorGUILayout.EndVertical();

			//インスペクター部分(右)
			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				InspectorFieldGUI();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndHorizontal();

		serializedObject.ApplyModifiedProperties();

		if (EditorGUI.EndChangeCheck())
		{
			UnityEditor.Undo.RecordObject(setting, "Edit CardDataBaseEditor");
			EditorUtility.SetDirty(setting);
		}
	}

	/// <summary>
	/// 上部部分のMenuコンテンツ
	/// </summary>
	void MenuGUI()
	{
		EditorGUILayout.BeginHorizontal();
		{
			if (GUI.changed == true) EditorGUILayout.LabelField("*更新後から保存されていません!");
			if (GUILayout.Button("保存", EditorStyles.miniButtonLeft, GUILayout.Width(100)))
			{
				EditorUtility.SetDirty(setting);
				foreach (CardData c in setting.CardList)
				{
					if (c == null) continue;

					string paths = AssetDatabase.GetAssetPath(c);
					string fileName = System.IO.Path.GetFileNameWithoutExtension(paths);

					c.cardEffect = fileName;

					EditorUtility.SetDirty(c);
					GUI.changed = false;
				}
				AssetDatabase.SaveAssets();
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// 左部分の選択
	/// </summary>
	void ObjectFieldGUI()
	{
		EditorGUILayout.Space(5);

		EditorGUILayout.BeginVertical(GUI.skin.box);
		objectFieldSlider = EditorGUILayout.BeginScrollView(objectFieldSlider, GUI.skin.box);

		reorderable.DoLayoutList();

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}

	/// <summary>
	/// 右部分のインスペクター
	/// </summary>
	void InspectorFieldGUI()
	{
		if (selectObjectIndex == -1) return;
		if (setting.CardList.Count == 0) return;
		if (setting.CardList[selectObjectIndex] == null)
		{
			//新規作成の部分
			EditorGUILayout.LabelField("ファイル名");
			createName = EditorGUILayout.TextField(createName);

			EditorGUILayout.Space(10);

			if (GUILayout.Button("新規作成"))
			{
				var path = "Assets/CardData/";
				path += createName + ".asset";

				CardData assetsData = ScriptableObject.CreateInstance<CardData>();
				AssetDatabase.CreateAsset(assetsData, path);
				setting.CardList[selectObjectIndex] = assetsData;
			}

			return;
		}

		inspectorFieldSlider = EditorGUILayout.BeginScrollView(inspectorFieldSlider, GUI.skin.box);

		//選択されているオブジェクトのデータ
		EditorGUILayout.LabelField("カードの位置: " + selectObjectIndex.ToString());
		EditorGUILayout.LabelField("カード情報の名前: " + setting.CardList[selectObjectIndex].ToString());
		{
			CardData cardData = setting.CardList[selectObjectIndex];

			//カードデータのパスからデータを取得
			string paths = AssetDatabase.GetAssetPath(setting.CardList[selectObjectIndex]);
			string fileName = System.IO.Path.GetFileNameWithoutExtension(paths);

			cardData.cardEffect = fileName;

			//カードタイプ毎に表示させる内容を変える
			cardData.type = (CardData.Type)EditorGUILayout.EnumPopup("カードの種類", cardData.type);

			//魔法と道具の共通設定項目
			if (cardData.type != CardData.Type.Equipment) cardData.useType = (CardData.UseType)EditorGUILayout.EnumPopup("使用制限", cardData.useType);

			if (cardData.type == CardData.Type.Magic)
			{
				//魔法と道具の共通設定項目
				cardData.costType = (CardData.CostType)EditorGUILayout.EnumPopup("コストタイプ", cardData.costType);
				EditorGUILayout.Space(10);

				//魔法の設定項目
				if (cardData.costType == CardData.CostType.Mana)
				{
					cardData.cooldown = 0;
					cardData.manaCost = EditorGUILayout.IntField("マナコスト", cardData.manaCost);
				}

				//道具の設定項目
				if (cardData.costType == CardData.CostType.Cooldown)
				{
					cardData.manaCost = 0;
					cardData.cooldown = EditorGUILayout.IntField("クールダウン", cardData.cooldown);
				}
			}
			else
			{
				//装備品の設定項目
				Separator(5);

				cardData.status.atk = EditorGUILayout.IntField("ATK", cardData.status.atk);
				cardData.status.def = EditorGUILayout.IntField("DEF", cardData.status.def);
				cardData.status.spd = EditorGUILayout.IntField("SPD", cardData.status.spd);
				cardData.status.avo = EditorGUILayout.IntField("AVO", cardData.status.avo);
				cardData.status.cri = EditorGUILayout.IntField("CRI", cardData.status.cri);

				EditorGUILayout.Space(10);

				cardData.overDamageArea = EditorGUILayout.FloatField("オーバーダメージ範囲", cardData.overDamageArea);
				cardData.overDamageMultiple = EditorGUILayout.FloatField("オーバーダメージ倍率", cardData.overDamageMultiple);

				cardData.manaCost = 0;
				cardData.cooldown = 0;
			}

			Separator(10);

			//全共通設定項目
			EditorGUILayout.PrefixLabel("名前");
			cardData.name = EditorGUILayout.TextField(cardData.name);
			EditorGUILayout.PrefixLabel("説明");
			cardData.lore = EditorGUILayout.TextArea(cardData.lore);

			EditorGUILayout.Space(5);
			EditorGUILayout.PrefixLabel("ティア");
			cardData.tier = EditorGUILayout.IntField(cardData.tier);
			EditorGUILayout.PrefixLabel("価値");
			cardData.price = EditorGUILayout.IntField(cardData.price);

			EditorGUILayout.Space(5);
			cardData.image = EditorGUILayout.ObjectField("イメージ", cardData.image, typeof(Sprite), true) as Sprite;


			Separator(10);

			EditorGUILayout.LabelField("Effect : " + cardData.cardEffect);

			//EditorGUILayout.PrefixLabel("効果メソッド");
			//cardData.cardEffect = EditorGUILayout.TextField(cardData.cardEffect);

			//EditorGUILayout.PrefixLabel("引数");
			//cardData.arg = EditorGUILayout.FloatField(cardData.arg);

			setting.CardList[selectObjectIndex] = cardData;
			setting.CardList[selectObjectIndex].name = cardData.name;
		}
		EditorGUILayout.EndScrollView();
	}

	/// <summary>
	/// 新しいカードデータ生成
	/// </summary>
	void CreateCardData(int indexs)
	{
		if (indexs < 0) indexs = 0;

		var path = "Assets/CardData/";
		path += (cardType == CardData.Type.Magic) ? "Magic/" : "Trap/";
		path += "NewData_" + setting.CardList.Count + ".asset";

		CardData assetsData = ScriptableObject.CreateInstance<CardData>();
		AssetDatabase.CreateAsset(assetsData, path);

		assetsData.type = cardType;
		assetsData.name = "New Data(" + setting.CardList.Count + ")";

		setting.CardList[indexs] = assetsData;
		Sort();
	}

	void Sort()
	{
		return;
	}

	private void Update()
	{
		Repaint();
	}

	/// <summary>
	/// スペースを開ける
	/// </summary>
	public static void Separator(int space)
	{
		GUILayout.Space(space);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(space);
	}
}
