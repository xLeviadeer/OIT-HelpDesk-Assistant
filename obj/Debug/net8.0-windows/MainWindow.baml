   M S B A M L   `   `   `  ����T  POIT HelpDesk Assistant v2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:OIT_HelpDesk_Assistant_v2OIT_HelpDesk_Assistant_v2  5   	   *    $OIT_HelpDesk_Assistant_v2.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   9local'clr-namespace:OIT_HelpDesk_Assistant_v2     5   	   -   5          � Title$  OIT HelpDesk Assistant��5   	   $	��700��5	   	   $
��Auto��6   $	��600��5
   	   $
��1500��5   	   $
��1200��6    � WindowStartupLocation$ CenterScreen=�5   	   $��Arial Round MT Bold)�5   	    � Icon$ /Resource/main.ico��5   	    � 	Resources 5       �� 
TargetType �� Property �� Value��%�  '��       '�� H     '�� �     '�� �    '�� G    �� 5   
   " ��6   .�5      ��  � ��$ 27��6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 5,5,5,0q�6'   �� 5       � ��$
 Left=�64   �� 5       � ��$ Center=�62   5      �� 5   
   " ��6   .�5      ��  � ��$ 5,3,5,3q�6'   �� 5       � ��$
 Left=�6;   �� 5       � ��$ Center=�69   �� 5       �@��	MaxLength$ 80��6*   5       �� 5!   
   " ��6   .�5"      ��  � ��$ 5,3,5,3q�6'   �� 5#       � ��$ Center=�69   �� 5$       � ��$
 Left=�6;   5%      �� 5&   
   " ��6   .�5'      ��  � ��$ 5,3,5,3q�6'   5(      5)      .��5,      �  � RowDefinitions 5-   
   �� 5.      -   5         ��Name$ TabControlRow��5.      $��30 �63   �� 5/      $��* �6   	 � ColumnDefinitions	 51   
   �� 52      $��* �6   �� 53      -   5        
 ��Name$
 RefreshColumn��53      $��75 �66   .+�55   
   �� -   5         ��Name$ AllTabs��55   W   $��0��6   $��2��6"   $��0��63   $��2��6C    �� TabStripPlacement$	 Top=�6h    �� SelectedIndex$ 0��6�   .@�56      �� $��	Phonetics��6   .��57      �  � ShowGridLines$ False��6   $��
10,0,10,10q�6-    58      �� 59      $��1* �6(   �� 5;      $��2* �6(   �� 5<      $��3* �6(   �� 5=      $��1* �6(   �� 5?      $��2* �6(   �� 5@      $��3* �6(   �� 5A      $��3* �6(   �� 5B      $��1* �6(   �� 5D      $��2* �6(   �� 5E      $	��50* �6(   	 5G      �� 5H      $��1* �6+   �� 5I      $��1* �6+   �� 5J      $��1* �6+   �� 5K      $��1* �6+   .+�5M      �� $��1��6    $��0��6-   $��4��6=   $��
Input Text��6Q   �� 5N      -   5         ��Name$ Phonetics_InputTextBox��5N   O   $��2��6   $��0��6+   $��4��6;   $��3,0,3,0q�6o    �� TextWrapping$ NoWrap=�6�    �� MaxLines$ 1��6�   6�   �� 5O      $��4��6    $��0��6-   $��4��6=   $��Phonetics Type��6Q   �� 5P      -   5         ��Name$ Phonetics_TypeComboBox��5P   P   $��5��6   $��0��6,   $��3��6<    �� 
IsReadOnly$
 True��6p   6�   �� 5Q      -   5         ��Name$ Phonetics_CreateButton��5Q   :   $��5��6   $��3��6*   $��
Create Set��6Z    � ToolTip$  Create a new phonetics set��6o   6�   �� 5R      -   5        $ Phonetics_DeleteButton��5R   :   $��6��6   $��3��6*   $��
Delete Set��6Z   $' !Delete the currently selected set��6o   6�   �� 5S      -	   5        $ Phonetics_ModifyButton��5S   :   $��6��6   $��2��6*   $��
Modify Set��6Z   $' !Modify the currently selected set��6o   6�   �� 5T      -
   5        $ Phonetics_ChangeButton��5T   N   $��6��6   $��0��6*   $��2��6:   $��Change to New Random��6n   $g a[Enter] If the current phonetics output is a Section, change it to a new random from that section��6�   ${�False��6�   6  �� 5U      $��8��6    $��0��6-   $��Ouput��6=   �� 5V      -   5        $ Phonetics_OuputTextBox��5V   O   $��9��6   $��0��6+   $��4��6;   $��5,5,5,5q�6o   $	��Top=�6�    �� 
IsReadOnly$
 True��6�   $ NoWrap=�6�   " �� VerticalScrollBarVisibility$
 Auto=�6�   ���	#FFEAEAEA6�   5W      5X      5Y      �� 5Z   
   -   5        $ RefreshButton��5Z   .   $��0��6   $��1��6   $��Refresh��6E   $) #Refreshes for any live-data updates��6W   6�   5[      5\      