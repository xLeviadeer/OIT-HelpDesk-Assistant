   M S B A M L   `   `   `  ����T  POIT HelpDesk Assistant v2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:OIT_HelpDesk_Assistant_v2OIT_HelpDesk_Assistant_v2  5      .    (OIT_HelpDesk_Assistant_v2.SearchPageGrid   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      9local'clr-namespace:OIT_HelpDesk_Assistant_v2     5        � 	Resources  5	       �� 
TargetType �� Property �� Value��%x  '��       '�� �     �� 5
   
   " ��6   .�5      ��  � ��$ 5,5,5,0q�6'   �� 5       � ��$
 Left=�64   �� 5       � ��$ Center=�62   5      �� 5   
   " ��6   .�5      ��  � ��$ 5,3,5,3q�6'   �� 5       � ��$
 Left=�6;   �� 5       � ��$ Center=�69   �� 5       �@��	MaxLength$ 43��6*   5      5      .��5      � .+�5   
   � $��
10,0,10,10q�6    � RowDefinitions 5      �� 5      $��1* �6    �� 5      $��2* �6    �� 5      $��3* �6    �� 5      $��1* �6    �� 5      $	��61* �6    .+�5       �� $��1��6   $��Search��6%   �� 5!      -   5         ��Name$ SearchTextBox��5!   #   $��2��6   $��3,0,3,0q�6:    �� TextWrapping$ NoWrap=�6L    �� MaxLines$ 1��6b   6o   �� 5"      $��4��6   $	��Top=�6(   $
��Auto=�6@   .��5#      � -   5         �Name$ 	ItemsGrid��5#      ��6*   �� *GridBorderThickness+	 �� RelativeSource	 �� 
 �� AncestorType
 UserControl5$      � 5%      -   5        $ 
BorderGrid��5%       $��4��6   $	��Top=�64   .+�5&      �� 
���   ���6-   �� *GridBorderThickness+	 �� 
 UserControl5'      5(      5)      5*      