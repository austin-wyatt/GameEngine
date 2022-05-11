## Scrollable Area

•	SetVisibleAreaPosition should be used instead of SetPosition or SetPositionFromAnchor. This moves the visible area and the underlying base area. The usage is similar to SetPositionFromAnchor. 
•	Objects inside of the base area (the base area is what moves with the scrollbar) should be added as children to the BaseComponent of the ScrollableArea. 