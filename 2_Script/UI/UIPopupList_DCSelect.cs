//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Popup list can be used to display pop-up menus and drop-down lists.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Popup List_DCSelect")]
public class UIPopupList_DCSelect : UIPopupList
{
	//자식 패널의 스케일 설정
	protected float fitScale
	{
		get
		{
			int viewItemCount = items.Count < 10 ? items.Count : 10;
			if (separatePanel)
			{
				var height = viewItemCount * (fontSize + padding.y) + padding.y;
				var size   = NGUITools.screenSize.y;
				if (height > size) return size / height;
			}
			else if (mPanel != null && mPanel.anchorCamera != null && mPanel.anchorCamera.orthographic)
			{
				var height = viewItemCount * (fontSize + padding.y) + padding.y;
				var size   = mPanel.height;
				if (height > size) return size / height;
			}
			return 1f;
		}
	}
	
	private UIScrollView mScrollView;
	private UIGrid       mGrid;

	private GameObject AddChild(GameObject parent, string name)
	{
		GameObject obj = new GameObject(name);
		obj.transform.parent        = parent.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale    = Vector3.one;
		obj.layer                   = parent.layer;

		return obj;
	}
	//드래그 함수 호출을 위해 눌러졌다는 조건을 변경
	protected override void OnItemPress (GameObject go, bool isPressed)
	{
		if (mScrollView && enabled && NGUITools.GetActive(gameObject))
		{
			mScrollView.Press(isPressed);
		}
	}
	
	//스크롤 뷰 선택 시 창이 닫히는 것을 막음
	protected override void OnSelect (bool isSelected)
	{
	}
	
	//DC 리스트 개별 자식 항목들을 드래그할 때 호출되어서 실제 스크롤을 움직임
	private	void OnDragScroll (GameObject go, Vector2 delta)
	{
		if (mScrollView && NGUITools.GetActive(this))
			mScrollView.Drag();
	}

	//하이라이트 오프셋 변경
	protected override Vector3 GetHighlightPosition()
	{
		if (mHighlightedLabel == null || mHighlight == null) return Vector3.zero;

		Vector4 border      = mHighlight.border;
		float   scaleFactor = 1f;

		var atl                      = atlas as INGUIAtlas;
		if (atl != null) scaleFactor = atl.pixelSize;

		float offsetX = border.x * scaleFactor;
		float offsetY = border.w * scaleFactor + mScrollView.transform.localPosition.y;
		return mHighlightedLabel.cachedTransform.localPosition + new Vector3(-offsetX, offsetY, 1f);
	}

	//기존 일반 리스트를 스크롤 뷰로 변경
	public override void Show()
    {
		if (enabled && NGUITools.GetActive(gameObject) && mChild == null && isValid && items.Count > 0)
		{
			mLabelList.Clear();
			StopCoroutine("CloseIfUnselected");

			// Ensure the popup's source has the selection
			UICamera.selectedObject = (UICamera.hoveredObject ?? gameObject);
			mSelection = UICamera.selectedObject;
			source = mSelection;

			if (source == null)
			{
				Debug.LogError("Popup list needs a source object...");
				return;
			}

			mOpenFrame = Time.frameCount;

			// Automatically locate the panel responsible for this object
			if (mPanel == null)
			{
				mPanel = UIPanel.Find(transform);
				if (mPanel == null) return;
			}

			// Calculate the dimensions of the object triggering the popup list so we can position it below it
			Vector3 min;
			Vector3 max;

			// Create the root object for the list
			mChild = new GameObject("Drop-down List");
			mChild.layer = gameObject.layer;

			if (separatePanel)
			{
				if (GetComponent<Collider>() != null)
				{
					Rigidbody rb = mChild.AddComponent<Rigidbody>();
					rb.isKinematic = true;
				}
				else if (GetComponent<Collider2D>() != null)
				{
					Rigidbody2D rb = mChild.AddComponent<Rigidbody2D>();
					rb.isKinematic = true;
				}

				var panel = mChild.AddComponent<UIPanel>();
				panel.depth        = 1000000;
				panel.sortingOrder = mPanel.sortingOrder;
				panel.clipping     = UIDrawCall.Clipping.ConstrainButDontClip;
			}

			current = this;

			var pTrans = mPanel.cachedTransform;
			Transform t = mChild.transform;
			t.parent = pTrans;
			Transform rootTrans = pTrans;

			if (separatePanel)
			{
				var root = mPanel.GetComponentInParent<UIRoot>();
				if (root == null && UIRoot.list.Count != 0) root = UIRoot.list[0];
				if (root != null) rootTrans = root.transform;
			}

			// Manually triggered popup list on some other game object
			if (openOn == OpenOn.Manual && mSelection != gameObject)
			{
				startingPosition = UICamera.lastEventPosition;
				min = pTrans.InverseTransformPoint(mPanel.anchorCamera.ScreenToWorldPoint(startingPosition));
				max = min;
				t.localPosition = min;
				startingPosition = t.position;
			}
			else
			{
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(pTrans, transform, false, false);
				min = bounds.min;
				max = bounds.max;
				t.localPosition = min;
				startingPosition = t.position;
			}

			StartCoroutine("CloseIfUnselected");

			var f = fitScale;
			t.localRotation = Quaternion.identity;
			t.localScale = new Vector3(f, f, f);

			int depth = separatePanel ? 0 : NGUITools.CalculateNextDepth(mPanel.gameObject);

			// Add a sprite for the background
			if (background2DSprite != null)
			{
				UI2DSprite sp2 = mChild.AddWidget<UI2DSprite>(depth);
				sp2.sprite2D = background2DSprite;
				mBackground = sp2;
			}
			else if (atlas != null) mBackground = NGUITools.AddSprite(mChild, atlas as INGUIAtlas, backgroundSprite, depth);
			else return;

			bool placeAbove = (position == Position.Above);

			if (position == Position.Auto)
			{
				UICamera cam = UICamera.FindCameraForLayer(mSelection.layer);

				if (cam != null)
				{
					Vector3 viewPos = cam.cachedCamera.WorldToViewportPoint(startingPosition);
					placeAbove = (viewPos.y < 0.5f);
				}
			}

			mBackground.pivot = UIWidget.Pivot.TopLeft;
			mBackground.color = backgroundColor;

			// We need to know the size of the background sprite for padding purposes
			Vector4 bgPadding = mBackground.border;
			mBgBorder = bgPadding.y;
			mBackground.cachedTransform.localPosition = Vector3.zero;

			// Add a sprite used for the selection
			if (highlight2DSprite != null)
			{
				UI2DSprite sp2 = mChild.AddWidget<UI2DSprite>(++depth);
				sp2.sprite2D = highlight2DSprite;
				mHighlight = sp2;
			}
			else if (atlas != null) mHighlight = NGUITools.AddSprite(mChild, atlas as INGUIAtlas, highlightSprite, ++depth);
			else return;

			float hlspHeight = 0f, hlspLeft = 0f;

			if (mHighlight.hasBorder)
			{
				hlspHeight = mHighlight.border.w;
				hlspLeft = mHighlight.border.x;
			}

			mHighlight.pivot = UIWidget.Pivot.TopLeft;
			mHighlight.color = highlightColor;

			//자식에 스크롤 뷰 추가
			if (mScrollView == null)
			{
				mScrollView                      = AddChild(mChild, "Scroll View").AddComponent<UIScrollView>();
				mScrollView.panel.depth          = 1000001;
				mScrollView.panel.clipping       = UIDrawCall.Clipping.SoftClip;
				mScrollView.movement             = UIScrollView.Movement.Vertical;
			}

			if (mGrid == null)
			{
				mGrid = AddChild(mScrollView.gameObject, "Grid").AddComponent<UIGrid>();
				mGrid.arrangement             = UIGrid.Arrangement.Vertical;
				mGrid.cellWidth               = 300;
				mGrid.cellHeight              = 80;
			}

			float labelHeight = activeFontSize * activeFontScale;
			float lineHeight = labelHeight + padding.y;
			float x = 0f, y = placeAbove ? bgPadding.y - padding.y - overlap : -padding.y - bgPadding.y + overlap;
			float contentHeight = bgPadding.y * 2f + padding.y;
			List<UILabel> labels = new List<UILabel>();

			// Clear the selection if it's no longer present
			if (!items.Contains(mSelectedItem))
				mSelectedItem = null;
			
			textColor = Color.gray * 1.3f;

			// Run through all items and create labels for each one
			for (int i = 0, imax = items.Count; i < imax; ++i)
			{
				string s = items[i];

				UILabel lbl = NGUITools.AddWidget<UILabel>(mGrid.gameObject, mBackground.depth + 2);
				lbl.name = i.ToString();
				lbl.pivot = UIWidget.Pivot.TopLeft;
				lbl.bitmapFont = bitmapFont as INGUIFont;
				lbl.trueTypeFont = trueTypeFont;
				lbl.fontSize = fontSize;
				lbl.fontStyle = fontStyle;
				lbl.text = isLocalized ? Localization.Get(s) : s;
				lbl.modifier = textModifier;
				lbl.color = textColor;
				lbl.cachedTransform.localPosition = new Vector3(bgPadding.x + padding.x - lbl.pivotOffset.x, y, -1f);
				lbl.overflowMethod = UILabel.Overflow.ResizeFreely;
				lbl.alignment = alignment;
				lbl.symbolStyle = NGUIText.SymbolStyle.Colored;
				labels.Add(lbl);

				if(i < 10)
					contentHeight += lineHeight;

				y -= lineHeight;
				x = Mathf.Max(x, lbl.printedSize.x);

				// Add an event listener
				UIEventListener listener = UIEventListener.Get(lbl.gameObject);
				listener.onHover   = OnItemHover;
				listener.onPress   = OnItemPress;
				listener.onClick   = OnItemClick;
				listener.onDrag    = OnDragScroll;
				listener.parameter = s;

				// Move the selection here if this is the right label
				if (mSelectedItem == s || (i == 0 && string.IsNullOrEmpty(mSelectedItem)))
					Highlight(lbl, true);

				// Add this label to the list
				mLabelList.Add(lbl);
			}
			
			//화면 밖으로 나간다면 스크롤을 맨 아래로 위치시킴
			if (items.Count > 10)
			{
				mScrollView.transform.localPosition = new Vector2(0, (items.Count - 10) * (fontSize + padding.y));
			}

			// The triggering widget's width should be the minimum allowed width
			x = Mathf.Min(x, 475);
			x = Mathf.Max(x, (max.x - min.x) - (bgPadding.x + padding.x) * 2f);

			float cx = x;
			Vector3 bcCenter = new Vector3(cx * 0.5f, -labelHeight * 0.5f, 0f);
			Vector3 bcSize = new Vector3(cx, (labelHeight + padding.y), 1f);

			// Run through all labels and add colliders
			for (int i = 0, imax = labels.Count; i < imax; ++i)
			{
				UILabel lbl = labels[i];
				NGUITools.AddWidgetCollider(lbl.gameObject);
				lbl.autoResizeBoxCollider = false;
				BoxCollider bc = lbl.GetComponent<BoxCollider>();

				if (bc != null)
				{
					bcCenter.z = bc.center.z;
					bc.center = bcCenter;
					bc.size = bcSize;
				}
				else
				{
					BoxCollider2D b2d = lbl.GetComponent<BoxCollider2D>();
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
					b2d.center = bcCenter;
#else
					b2d.offset = bcCenter;
#endif
					b2d.size = bcSize;
				}
			}

			int lblWidth = Mathf.RoundToInt(x);
			x += (bgPadding.x + padding.x) * 2f;
			y -= bgPadding.y;

			// Scale the background sprite to envelop the entire set of items
			mBackground.width                = Mathf.RoundToInt(x);
			mBackground.height               = Mathf.RoundToInt(contentHeight);
			mScrollView.panel.baseClipRegion = new Vector4(mScrollView.transform.position.x + x * 0.5f, mScrollView.transform.position.y - mScrollView.transform.localPosition.y - contentHeight * 0.5f, x, contentHeight);

			// Set the label width to make alignment work
			for (int i = 0, imax = labels.Count; i < imax; ++i)
			{
				UILabel lbl = labels[i];
				lbl.overflowMethod = UILabel.Overflow.ShrinkContent;
				lbl.width = lblWidth;
			}

			// Scale the highlight sprite to envelop a single item

			float scaleFactor = 2f;
			var atl = atlas as INGUIAtlas;
			if (atl != null) scaleFactor *= atl.pixelSize;

			float w = x - (bgPadding.x + padding.x) * 2f + hlspLeft * scaleFactor;
			float h = labelHeight + hlspHeight * scaleFactor;
			mHighlight.width = Mathf.RoundToInt(w);
			mHighlight.height = Mathf.RoundToInt(h);

			// If the list should be animated, let's animate it by expanding it
			if (isAnimated)
			{
				AnimateColor(mBackground);

				if (Time.timeScale == 0f || Time.timeScale >= 0.1f)
				{
					float bottom = y + labelHeight;
					Animate(mHighlight, placeAbove, bottom);
					for (int i = 0, imax = labels.Count; i < imax; ++i)
						Animate(labels[i], placeAbove, bottom);
					AnimateScale(mBackground, placeAbove, bottom);
				}
			}

			// If we need to place the popup list above the item, we need to reposition everything by the size of the list
			if (placeAbove)
			{
				var bgY = bgPadding.y * f;
				min.y = max.y - bgPadding.y * f;
				max.y = min.y + (mBackground.height - bgPadding.y * 2f) * f;
				max.x = min.x + mBackground.width * f;
				t.localPosition = new Vector3(min.x, max.y - bgY, min.z);
			}
			else
			{
				max.y = min.y + bgPadding.y * f;
				min.y = max.y - mBackground.height * f;
				max.x = min.x + mBackground.width * f;
			}

			var absoluteParent = mPanel;// UIRoot.list[0].GetComponent<UIPanel>();

			for (;;)
			{
				var p = absoluteParent.parent;
				if (p == null) break;
				var pp = p.GetComponentInParent<UIPanel>();
				if (pp == null) break;
				absoluteParent = pp;
			}

			if (pTrans != null)
			{
				min = pTrans.TransformPoint(min);
				max = pTrans.TransformPoint(max);
				min = absoluteParent.cachedTransform.InverseTransformPoint(min);
				max = absoluteParent.cachedTransform.InverseTransformPoint(max);
				var adj = UIRoot.GetPixelSizeAdjustment(gameObject);
				min /= adj;
				max /= adj;
			}

			// Ensure that everything fits into the panel's visible range
			var offset = absoluteParent.CalculateConstrainOffset(min, max);
			var pos = t.localPosition + offset;
			pos.x = Mathf.Round(pos.x);
			pos.y = Mathf.Round(pos.y);
			t.localPosition = pos;
			t.parent = rootTrans;
		}
		else OnSelect(false);
    }
}
