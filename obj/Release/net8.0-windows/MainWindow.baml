   M S B A M L   `   `   `  ���� T  POIT HelpDesk Assistant v2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:OIT_HelpDesk_Assistant_v2OIT_HelpDesk_Assistant_v2  *    $OIT_HelpDesk_Assistant_v2.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     9local'clr-namespace:OIT_HelpDesk_Assistant_v2     -     � Title$  OIT HelpDesk Assistant��$	��700��$
��Auto��$	��600��$
��1500��$
��1200�� � WindowStartupLocation$ CenterScreen=�$��Arial Round MT Bold)� � Icon$ /Resource/main.ico�� � 	Resources  �� 
TargetType �� Property �� Value��%�  '��       '�� #     '�� }     '�� �     '�� Q    �� " ��.���  � ��$ 27���� " ��.���  � ��$ 5,5,5,0q���  � ��$
 Left=���  � ��$ Center=��� " ��.���  � ��$ 5,3,5,3q���  � ��$
 Left=���  � ��$ Center=���  �@��	MaxLength$ 80���� " ��.���  � ��$ 5,3,5,3q���  � ��$ Center=���  � ��$
 Left=��� " ��.���  � ��$ 5,3,5,3q�.���  � RowDefinitions �� -    ��Name$ TabControlRow��$��30 ��� $��* �	 � ColumnDefinitions	 �� $��* ��� -   
 ��Name$
 RefreshColumn��$��75 �.+��� -    ��Name$ AllTabs��$��0��$��2��$��0��$��2�� �� TabStripPlacement$	 Top=� �� SelectedIndex$ 0��.@��� $��	Phonetics��.���  � ShowGridLines$ False��$��
10,0,10,10q� �� $��1* ��� $��2* ��� $��3* ��� $��1* ��� $��2* ��� $��3* ��� $��3* ��� $��1* ��� $��2* ��� $	��50* �	 �� $��1* ��� $��1* ��� $��1* ��� $��1* �.+��� $��1��$��0��$��4��$��
Input Text���� -    ��Name$ Phonetics_InputTextBox��$��2��$��0��$��4��$��3,0,3,0q� �� TextWrapping$ NoWrap=� �� MaxLines$ 1���� $��4��$��0��$��4��$��Phonetics Type���� -    ��Name$ Phonetics_TypeComboBox��$��5��$��0��$��3�� �� 
IsReadOnly$
 True���� -    ��Name$ Phonetics_CreateButton��$��5��$��3��$��
Create Set�� � ToolTip$  Create a new phonetics set���� -   $ Phonetics_DeleteButton��$��6��$��3��$��
Delete Set��$' !Delete the currently selected set���� -	   $ Phonetics_ModifyButton��$��6��$��2��$��
Modify Set��$' !Modify the currently selected set���� -
   $ Phonetics_ChangeButton��$��6��$��0��$��2��$��Change to New Random��$g a[Enter] If the current phonetics output is a Section, change it to a new random from that section��${�False���� $��8��$��0��$��Ouput���� -   $ Phonetics_OuputTextBox��$��9��$��0��$��4��$��5,5,5,5q�$	��Top=� �� 
IsReadOnly$
 True��$ NoWrap=�" �� VerticalScrollBarVisibility$
 Auto=����	#FFEAEAEA�� -   $ RefreshButton��$��0��$��1��$��Refresh��$) #Refreshes for any live-data updates��