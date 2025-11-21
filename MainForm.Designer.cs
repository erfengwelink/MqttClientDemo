namespace MqttClientDemo
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            grpConnectionSettings = new GroupBox();
            grpConnectionStatus = new GroupBox();
            lblConnectionStatus = new Label();
            txtClientId = new TextBox();
            label4 = new Label();
            txtPassword = new TextBox();
            txtUsername = new TextBox();
            label3 = new Label();
            label2 = new Label();
            txtPort = new TextBox();
            txtBrokerAddress = new TextBox();
            label1 = new Label();
            label7 = new Label();
            chkCleanSession = new CheckBox();
            btnConnect = new Button();
            btnDisconnect = new Button();
            grpWillMessage = new GroupBox();
            chkWillRetain = new CheckBox();
            cmbWillQoS = new ComboBox();
            label6 = new Label();
            txtWillMessage = new TextBox();
            txtWillTopic = new TextBox();
            label5 = new Label();
            chkUseWill = new CheckBox();
            tabPage2 = new TabPage();
            grpPublish = new GroupBox();
            chkRetain = new CheckBox();
            cmbPublishQoS = new ComboBox();
            label12 = new Label();
            btnPublish = new Button();
            txtPublishMessage = new TextBox();
            label10 = new Label();
            txtPublishTopic = new TextBox();
            label9 = new Label();
            grpSubscribe = new GroupBox();
            btnUnsubscribe = new Button();
            lstSubscribedTopics = new ListBox();
            cmbSubscribeQoS = new ComboBox();
            btnSubscribe = new Button();
            txtSubscribeTopic = new TextBox();
            label11 = new Label();
            label8 = new Label();
            grpReceivedMessages = new GroupBox();
            lstReceivedMessages = new ListBox();
            tabPage3 = new TabPage();
            grpLog = new GroupBox();
            payloadDisplayLabel = new Label();
            rdoPayloadHex = new RadioButton();
            rdoPayloadString = new RadioButton();
            txtLog = new TextBox();
            tabPage4 = new TabPage();
            grpStrategyList = new GroupBox();
            lstStrategies = new ListBox();
            btnAddStrategy = new Button();
            btnAddStrategyPlus = new Button();
            btnEditStrategy = new Button();
            btnDeleteStrategy = new Button();
            grpStrategyDetail = new GroupBox();
            tabStrategyDetail = new TabControl();
            tabPageStrategyInfo = new TabPage();
            labelStrategyDetail = new Label();
            tabPagePayload = new TabPage();
            labelFormatSelector = new Label();
            rdoStringMode = new RadioButton();
            rdoHexMode = new RadioButton();
            txtFullStrategyPayload = new TextBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            grpConnectionSettings.SuspendLayout();
            grpConnectionStatus.SuspendLayout();
            grpWillMessage.SuspendLayout();
            tabPage2.SuspendLayout();
            grpPublish.SuspendLayout();
            grpSubscribe.SuspendLayout();
            grpReceivedMessages.SuspendLayout();
            tabPage3.SuspendLayout();
            grpLog.SuspendLayout();
            tabPage4.SuspendLayout();
            grpStrategyList.SuspendLayout();
            grpStrategyDetail.SuspendLayout();
            tabStrategyDetail.SuspendLayout();
            tabPageStrategyInfo.SuspendLayout();
            tabPagePayload.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(800, 600);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(grpConnectionSettings);
            tabPage1.Controls.Add(grpWillMessage);
            tabPage1.Location = new Point(4, 26);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(792, 570);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "连接设置";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // grpConnectionSettings
            // 
            grpConnectionSettings.Controls.Add(grpConnectionStatus);
            grpConnectionSettings.Controls.Add(txtClientId);
            grpConnectionSettings.Controls.Add(label4);
            grpConnectionSettings.Controls.Add(txtPassword);
            grpConnectionSettings.Controls.Add(txtUsername);
            grpConnectionSettings.Controls.Add(label3);
            grpConnectionSettings.Controls.Add(label2);
            grpConnectionSettings.Controls.Add(txtPort);
            grpConnectionSettings.Controls.Add(txtBrokerAddress);
            grpConnectionSettings.Controls.Add(label1);
            grpConnectionSettings.Controls.Add(label7);
            grpConnectionSettings.Controls.Add(chkCleanSession);
            grpConnectionSettings.Controls.Add(btnConnect);
            grpConnectionSettings.Controls.Add(btnDisconnect);
            grpConnectionSettings.Location = new Point(6, 6);
            grpConnectionSettings.Name = "grpConnectionSettings";
            grpConnectionSettings.Size = new Size(780, 250);
            grpConnectionSettings.TabIndex = 0;
            grpConnectionSettings.TabStop = false;
            grpConnectionSettings.Text = "连接设置";
            // 
            // grpConnectionStatus
            // 
            grpConnectionStatus.BackColor = Color.Red;
            grpConnectionStatus.Controls.Add(lblConnectionStatus);
            grpConnectionStatus.Location = new Point(505, 40);
            grpConnectionStatus.Name = "grpConnectionStatus";
            grpConnectionStatus.Size = new Size(179, 50);
            grpConnectionStatus.TabIndex = 1;
            grpConnectionStatus.TabStop = false;
            grpConnectionStatus.Text = "连接状态";
            grpConnectionStatus.Enter += grpConnectionStatus_Enter;
            // 
            // lblConnectionStatus
            // 
            lblConnectionStatus.AutoSize = true;
            lblConnectionStatus.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblConnectionStatus.Location = new Point(55, 16);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(58, 22);
            lblConnectionStatus.TabIndex = 0;
            lblConnectionStatus.Text = "已断开";
            // 
            // txtClientId
            // 
            txtClientId.Location = new Point(100, 80);
            txtClientId.Name = "txtClientId";
            txtClientId.Size = new Size(200, 23);
            txtClientId.TabIndex = 4;
            txtClientId.TextChanged += txtBrokerAddress_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 83);
            label4.Name = "label4";
            label4.Size = new Size(57, 17);
            label4.TabIndex = 3;
            label4.Text = "客户端ID";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(100, 130);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(200, 23);
            txtPassword.TabIndex = 6;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(100, 105);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(200, 23);
            txtUsername.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(10, 133);
            label3.Name = "label3";
            label3.Size = new Size(68, 17);
            label3.TabIndex = 2;
            label3.Text = "密码 (可选)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 108);
            label2.Name = "label2";
            label2.Size = new Size(80, 17);
            label2.TabIndex = 1;
            label2.Text = "用户名 (可选)";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(350, 55);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(80, 23);
            txtPort.TabIndex = 2;
            txtPort.TextChanged += txtPort_TextChanged;
            // 
            // txtBrokerAddress
            // 
            txtBrokerAddress.Location = new Point(100, 55);
            txtBrokerAddress.Name = "txtBrokerAddress";
            txtBrokerAddress.Size = new Size(200, 23);
            txtBrokerAddress.TabIndex = 1;
            txtBrokerAddress.TextChanged += txtBrokerAddress_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 58);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 0;
            label1.Text = "代理地址";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(310, 58);
            label7.Name = "label7";
            label7.Size = new Size(32, 17);
            label7.TabIndex = 10;
            label7.Text = "端口";
            // 
            // chkCleanSession
            // 
            chkCleanSession.AutoSize = true;
            chkCleanSession.Location = new Point(100, 160);
            chkCleanSession.Name = "chkCleanSession";
            chkCleanSession.Size = new Size(75, 21);
            chkCleanSession.TabIndex = 7;
            chkCleanSession.Text = "清理会话";
            chkCleanSession.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(100, 200);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(100, 30);
            btnConnect.TabIndex = 8;
            btnConnect.Text = "连接";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += BtnConnect_Click;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Enabled = false;
            btnDisconnect.Location = new Point(220, 200);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(100, 30);
            btnDisconnect.TabIndex = 9;
            btnDisconnect.Text = "断开连接";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // grpWillMessage
            // 
            grpWillMessage.Controls.Add(chkWillRetain);
            grpWillMessage.Controls.Add(cmbWillQoS);
            grpWillMessage.Controls.Add(label6);
            grpWillMessage.Controls.Add(txtWillMessage);
            grpWillMessage.Controls.Add(txtWillTopic);
            grpWillMessage.Controls.Add(label5);
            grpWillMessage.Controls.Add(chkUseWill);
            grpWillMessage.Location = new Point(6, 262);
            grpWillMessage.Name = "grpWillMessage";
            grpWillMessage.Size = new Size(780, 150);
            grpWillMessage.TabIndex = 2;
            grpWillMessage.TabStop = false;
            grpWillMessage.Text = "遗嘱消息 (Last Will)";
            // 
            // chkWillRetain
            // 
            chkWillRetain.AutoSize = true;
            chkWillRetain.Enabled = false;
            chkWillRetain.Location = new Point(350, 85);
            chkWillRetain.Name = "chkWillRetain";
            chkWillRetain.Size = new Size(75, 21);
            chkWillRetain.TabIndex = 14;
            chkWillRetain.Text = "保留消息";
            chkWillRetain.UseVisualStyleBackColor = true;
            // 
            // cmbWillQoS
            // 
            cmbWillQoS.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWillQoS.Enabled = false;
            cmbWillQoS.FormattingEnabled = true;
            cmbWillQoS.Items.AddRange(new object[] { "QoS 0 (最多一次)", "QoS 1 (至少一次)", "QoS 2 (仅一次)" });
            cmbWillQoS.Location = new Point(240, 85);
            cmbWillQoS.Name = "cmbWillQoS";
            cmbWillQoS.Size = new Size(100, 25);
            cmbWillQoS.TabIndex = 13;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(200, 88);
            label6.Name = "label6";
            label6.Size = new Size(33, 17);
            label6.TabIndex = 12;
            label6.Text = "QoS";
            // 
            // txtWillMessage
            // 
            txtWillMessage.Enabled = false;
            txtWillMessage.Location = new Point(440, 55);
            txtWillMessage.Name = "txtWillMessage";
            txtWillMessage.Size = new Size(300, 23);
            txtWillMessage.TabIndex = 11;
            // 
            // txtWillTopic
            // 
            txtWillTopic.Enabled = false;
            txtWillTopic.Location = new Point(100, 55);
            txtWillTopic.Name = "txtWillTopic";
            txtWillTopic.Size = new Size(120, 23);
            txtWillTopic.TabIndex = 10;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(360, 58);
            label5.Name = "label5";
            label5.Size = new Size(80, 17);
            label5.TabIndex = 9;
            label5.Text = "遗嘱消息内容";
            // 
            // chkUseWill
            // 
            chkUseWill.AutoSize = true;
            chkUseWill.Location = new Point(10, 30);
            chkUseWill.Name = "chkUseWill";
            chkUseWill.Size = new Size(99, 21);
            chkUseWill.TabIndex = 8;
            chkUseWill.Text = "启用遗嘱消息";
            chkUseWill.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(grpPublish);
            tabPage2.Controls.Add(grpSubscribe);
            tabPage2.Controls.Add(grpReceivedMessages);
            tabPage2.Location = new Point(4, 26);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(792, 570);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "消息收发";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // grpPublish
            // 
            grpPublish.Controls.Add(chkRetain);
            grpPublish.Controls.Add(cmbPublishQoS);
            grpPublish.Controls.Add(label12);
            grpPublish.Controls.Add(btnPublish);
            grpPublish.Controls.Add(txtPublishMessage);
            grpPublish.Controls.Add(label10);
            grpPublish.Controls.Add(txtPublishTopic);
            grpPublish.Controls.Add(label9);
            grpPublish.Location = new Point(6, 6);
            grpPublish.Name = "grpPublish";
            grpPublish.Size = new Size(380, 250);
            grpPublish.TabIndex = 2;
            grpPublish.TabStop = false;
            grpPublish.Text = "发布消息";
            // 
            // chkRetain
            // 
            chkRetain.AutoSize = true;
            chkRetain.Location = new Point(220, 90);
            chkRetain.Name = "chkRetain";
            chkRetain.Size = new Size(75, 21);
            chkRetain.TabIndex = 17;
            chkRetain.Text = "保留消息";
            chkRetain.UseVisualStyleBackColor = true;
            // 
            // cmbPublishQoS
            // 
            cmbPublishQoS.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPublishQoS.FormattingEnabled = true;
            cmbPublishQoS.Items.AddRange(new object[] { "QoS 0 (最多一次)", "QoS 1 (至少一次)", "QoS 2 (仅一次)" });
            cmbPublishQoS.Location = new Point(100, 89);
            cmbPublishQoS.Name = "cmbPublishQoS";
            cmbPublishQoS.Size = new Size(110, 25);
            cmbPublishQoS.TabIndex = 16;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(10, 92);
            label12.Name = "label12";
            label12.Size = new Size(33, 17);
            label12.TabIndex = 15;
            label12.Text = "QoS";
            // 
            // btnPublish
            // 
            btnPublish.Location = new Point(100, 200);
            btnPublish.Name = "btnPublish";
            btnPublish.Size = new Size(100, 30);
            btnPublish.TabIndex = 14;
            btnPublish.Text = "发布";
            btnPublish.UseVisualStyleBackColor = true;
            btnPublish.Click += btnPublish_Click;
            // 
            // txtPublishMessage
            // 
            txtPublishMessage.Location = new Point(10, 120);
            txtPublishMessage.Multiline = true;
            txtPublishMessage.Name = "txtPublishMessage";
            txtPublishMessage.Size = new Size(360, 70);
            txtPublishMessage.TabIndex = 13;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(10, 100);
            label10.Name = "label10";
            label10.Size = new Size(56, 17);
            label10.TabIndex = 12;
            label10.Text = "消息内容";
            // 
            // txtPublishTopic
            // 
            txtPublishTopic.Location = new Point(100, 55);
            txtPublishTopic.Name = "txtPublishTopic";
            txtPublishTopic.Size = new Size(260, 23);
            txtPublishTopic.TabIndex = 11;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(10, 58);
            label9.Name = "label9";
            label9.Size = new Size(76, 17);
            label9.TabIndex = 10;
            label9.Text = "主题 (Topic)";
            // 
            // grpSubscribe
            // 
            grpSubscribe.Controls.Add(btnUnsubscribe);
            grpSubscribe.Controls.Add(lstSubscribedTopics);
            grpSubscribe.Controls.Add(cmbSubscribeQoS);
            grpSubscribe.Controls.Add(btnSubscribe);
            grpSubscribe.Controls.Add(txtSubscribeTopic);
            grpSubscribe.Controls.Add(label11);
            grpSubscribe.Controls.Add(label8);
            grpSubscribe.Location = new Point(400, 6);
            grpSubscribe.Name = "grpSubscribe";
            grpSubscribe.Size = new Size(380, 250);
            grpSubscribe.TabIndex = 3;
            grpSubscribe.TabStop = false;
            grpSubscribe.Text = "订阅主题";
            // 
            // btnUnsubscribe
            // 
            btnUnsubscribe.Enabled = false;
            btnUnsubscribe.Location = new Point(270, 85);
            btnUnsubscribe.Name = "btnUnsubscribe";
            btnUnsubscribe.Size = new Size(80, 30);
            btnUnsubscribe.TabIndex = 19;
            btnUnsubscribe.Text = "取消订阅";
            btnUnsubscribe.UseVisualStyleBackColor = true;
            btnUnsubscribe.Click += btnUnsubscribe_Click;
            // 
            // lstSubscribedTopics
            // 
            lstSubscribedTopics.FormattingEnabled = true;
            lstSubscribedTopics.ItemHeight = 17;
            lstSubscribedTopics.Location = new Point(10, 120);
            lstSubscribedTopics.Name = "lstSubscribedTopics";
            lstSubscribedTopics.Size = new Size(360, 123);
            lstSubscribedTopics.TabIndex = 18;
            // 
            // cmbSubscribeQoS
            // 
            cmbSubscribeQoS.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSubscribeQoS.FormattingEnabled = true;
            cmbSubscribeQoS.Items.AddRange(new object[] { "QoS 0 (最多一次)", "QoS 1 (至少一次)", "QoS 2 (仅一次)" });
            cmbSubscribeQoS.Location = new Point(180, 55);
            cmbSubscribeQoS.Name = "cmbSubscribeQoS";
            cmbSubscribeQoS.Size = new Size(110, 25);
            cmbSubscribeQoS.TabIndex = 16;
            // 
            // btnSubscribe
            // 
            btnSubscribe.Location = new Point(290, 50);
            btnSubscribe.Name = "btnSubscribe";
            btnSubscribe.Size = new Size(80, 30);
            btnSubscribe.TabIndex = 15;
            btnSubscribe.Text = "订阅";
            btnSubscribe.UseVisualStyleBackColor = true;
            btnSubscribe.Click += btnSubscribe_Click;
            // 
            // txtSubscribeTopic
            // 
            txtSubscribeTopic.Location = new Point(100, 55);
            txtSubscribeTopic.Name = "txtSubscribeTopic";
            txtSubscribeTopic.Size = new Size(70, 23);
            txtSubscribeTopic.TabIndex = 14;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(160, 58);
            label11.Name = "label11";
            label11.Size = new Size(33, 17);
            label11.TabIndex = 13;
            label11.Text = "QoS";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(10, 58);
            label8.Name = "label8";
            label8.Size = new Size(100, 17);
            label8.TabIndex = 12;
            label8.Text = "订阅主题 (Topic)";
            // 
            // grpReceivedMessages
            // 
            grpReceivedMessages.Controls.Add(lstReceivedMessages);
            grpReceivedMessages.Location = new Point(6, 262);
            grpReceivedMessages.Name = "grpReceivedMessages";
            grpReceivedMessages.Size = new Size(774, 300);
            grpReceivedMessages.TabIndex = 4;
            grpReceivedMessages.TabStop = false;
            grpReceivedMessages.Text = "收到的消息";
            // 
            // lstReceivedMessages
            // 
            lstReceivedMessages.FormattingEnabled = true;
            lstReceivedMessages.ItemHeight = 17;
            lstReceivedMessages.Location = new Point(10, 25);
            lstReceivedMessages.Name = "lstReceivedMessages";
            lstReceivedMessages.Size = new Size(750, 259);
            lstReceivedMessages.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(grpLog);
            tabPage3.Location = new Point(4, 26);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(792, 570);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "日志";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // grpLog
            // 
            grpLog.Controls.Add(payloadDisplayLabel);
            grpLog.Controls.Add(rdoPayloadHex);
            grpLog.Controls.Add(rdoPayloadString);
            grpLog.Controls.Add(txtLog);
            grpLog.Dock = DockStyle.Fill;
            grpLog.Location = new Point(3, 3);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(786, 564);
            grpLog.TabIndex = 0;
            grpLog.TabStop = false;
            grpLog.Text = "系统日志";
            // 
            // payloadDisplayLabel
            // 
            payloadDisplayLabel.AutoSize = true;
            payloadDisplayLabel.Location = new Point(658, 30);
            payloadDisplayLabel.Name = "payloadDisplayLabel";
            payloadDisplayLabel.Size = new Size(105, 17);
            payloadDisplayLabel.TabIndex = 3;
            payloadDisplayLabel.Text = "Payload显示方式:";
            payloadDisplayLabel.Click += payloadDisplayLabel_Click;
            // 
            // rdoPayloadHex
            // 
            rdoPayloadHex.AutoSize = true;
            rdoPayloadHex.Location = new Point(665, 77);
            rdoPayloadHex.Name = "rdoPayloadHex";
            rdoPayloadHex.Size = new Size(98, 21);
            rdoPayloadHex.TabIndex = 2;
            rdoPayloadHex.Text = "十六进制格式";
            rdoPayloadHex.UseVisualStyleBackColor = true;
            rdoPayloadHex.CheckedChanged += rdoHexMode_CheckedChanged;
            // 
            // rdoPayloadString
            // 
            rdoPayloadString.AutoSize = true;
            rdoPayloadString.Checked = true;
            rdoPayloadString.Location = new Point(665, 50);
            rdoPayloadString.Name = "rdoPayloadString";
            rdoPayloadString.Size = new Size(86, 21);
            rdoPayloadString.TabIndex = 1;
            rdoPayloadString.TabStop = true;
            rdoPayloadString.Text = "字符串格式";
            rdoPayloadString.UseVisualStyleBackColor = true;
            rdoPayloadString.CheckedChanged += rdoStringMode_CheckedChanged;
            // 
            // txtLog
            // 
            txtLog.Dock = DockStyle.Fill;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.Location = new Point(3, 19);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = ScrollBars.Both;
            txtLog.Size = new Size(780, 542);
            txtLog.TabIndex = 0;
            txtLog.TextChanged += txtLog_TextChanged;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(grpStrategyList);
            tabPage4.Controls.Add(grpStrategyDetail);
            tabPage4.Location = new Point(4, 26);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(792, 570);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "策略配置";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // grpStrategyList
            // 
            grpStrategyList.Controls.Add(lstStrategies);
            grpStrategyList.Controls.Add(btnAddStrategy);
            grpStrategyList.Controls.Add(btnAddStrategyPlus);
            grpStrategyList.Controls.Add(btnEditStrategy);
            grpStrategyList.Controls.Add(btnDeleteStrategy);
            grpStrategyList.Location = new Point(6, 6);
            grpStrategyList.Name = "grpStrategyList";
            grpStrategyList.Size = new Size(300, 558);
            grpStrategyList.TabIndex = 0;
            grpStrategyList.TabStop = false;
            grpStrategyList.Text = "策略列表";
            // 
            // lstStrategies
            // 
            lstStrategies.FormattingEnabled = true;
            lstStrategies.ItemHeight = 17;
            lstStrategies.Location = new Point(6, 22);
            lstStrategies.Name = "lstStrategies";
            lstStrategies.Size = new Size(288, 293);
            lstStrategies.TabIndex = 0;
            lstStrategies.SelectedIndexChanged += lstStrategies_SelectedIndexChanged;
            // 
            // btnAddStrategy
            // 
            btnAddStrategy.Location = new Point(6, 384);
            btnAddStrategy.Name = "btnAddStrategy";
            btnAddStrategy.Size = new Size(100, 30);
            btnAddStrategy.TabIndex = 1;
            btnAddStrategy.Text = "添加策略";
            btnAddStrategy.UseVisualStyleBackColor = true;
            btnAddStrategy.Click += btnAddStrategy_Click;
            // 
            // btnAddStrategyPlus
            // 
            btnAddStrategyPlus.Location = new Point(112, 384);
            btnAddStrategyPlus.Name = "btnAddStrategyPlus";
            btnAddStrategyPlus.Size = new Size(28, 30);
            btnAddStrategyPlus.TabIndex = 4;
            btnAddStrategyPlus.Text = "+";
            btnAddStrategyPlus.UseVisualStyleBackColor = true;
            btnAddStrategyPlus.Click += btnAddStrategy_Click;
            // 
            // btnEditStrategy
            // 
            btnEditStrategy.Location = new Point(6, 420);
            btnEditStrategy.Name = "btnEditStrategy";
            btnEditStrategy.Size = new Size(100, 30);
            btnEditStrategy.TabIndex = 2;
            btnEditStrategy.Text = "编辑策略";
            btnEditStrategy.UseVisualStyleBackColor = true;
            btnEditStrategy.Click += btnEditStrategy_Click;
            // 
            // btnDeleteStrategy
            // 
            btnDeleteStrategy.Location = new Point(6, 456);
            btnDeleteStrategy.Name = "btnDeleteStrategy";
            btnDeleteStrategy.Size = new Size(100, 30);
            btnDeleteStrategy.TabIndex = 3;
            btnDeleteStrategy.Text = "删除策略";
            btnDeleteStrategy.UseVisualStyleBackColor = true;
            btnDeleteStrategy.Click += btnDeleteStrategy_Click;
            // 
            // grpStrategyDetail
            // 
            grpStrategyDetail.Controls.Add(tabStrategyDetail);
            grpStrategyDetail.Location = new Point(312, 6);
            grpStrategyDetail.Name = "grpStrategyDetail";
            grpStrategyDetail.Size = new Size(474, 558);
            grpStrategyDetail.TabIndex = 1;
            grpStrategyDetail.TabStop = false;
            grpStrategyDetail.Text = "策略详情";
            // 
            // tabStrategyDetail
            // 
            tabStrategyDetail.Controls.Add(tabPageStrategyInfo);
            tabStrategyDetail.Controls.Add(tabPagePayload);
            tabStrategyDetail.Location = new Point(6, 22);
            tabStrategyDetail.Name = "tabStrategyDetail";
            tabStrategyDetail.SelectedIndex = 0;
            tabStrategyDetail.Size = new Size(462, 530);
            tabStrategyDetail.TabIndex = 0;
            // 
            // tabPageStrategyInfo
            // 
            tabPageStrategyInfo.Controls.Add(labelStrategyDetail);
            tabPageStrategyInfo.Location = new Point(4, 26);
            tabPageStrategyInfo.Name = "tabPageStrategyInfo";
            tabPageStrategyInfo.Padding = new Padding(3);
            tabPageStrategyInfo.Size = new Size(454, 500);
            tabPageStrategyInfo.TabIndex = 0;
            tabPageStrategyInfo.Text = "策略信息";
            tabPageStrategyInfo.UseVisualStyleBackColor = true;
            // 
            // labelStrategyDetail
            // 
            labelStrategyDetail.Dock = DockStyle.Fill;
            labelStrategyDetail.Location = new Point(3, 3);
            labelStrategyDetail.Name = "labelStrategyDetail";
            labelStrategyDetail.Size = new Size(448, 494);
            labelStrategyDetail.TabIndex = 0;
            labelStrategyDetail.Text = "请选择或添加一个策略来查看或编辑详情";
            labelStrategyDetail.Click += labelStrategyDetail_Click;
            // 
            // tabPagePayload
            // 
            tabPagePayload.Controls.Add(labelFormatSelector);
            tabPagePayload.Controls.Add(rdoStringMode);
            tabPagePayload.Controls.Add(rdoHexMode);
            tabPagePayload.Controls.Add(txtFullStrategyPayload);
            tabPagePayload.Location = new Point(4, 26);
            tabPagePayload.Name = "tabPagePayload";
            tabPagePayload.Padding = new Padding(3);
            tabPagePayload.Size = new Size(454, 500);
            tabPagePayload.TabIndex = 1;
            tabPagePayload.Text = "完整策略配置报文";
            tabPagePayload.UseVisualStyleBackColor = true;
            // 
            // labelFormatSelector
            // 
            labelFormatSelector.AutoSize = true;
            labelFormatSelector.Location = new Point(9, 9);
            labelFormatSelector.Name = "labelFormatSelector";
            labelFormatSelector.Size = new Size(83, 17);
            labelFormatSelector.TabIndex = 3;
            labelFormatSelector.Text = "选择报文格式:";
            // 
            // rdoStringMode
            // 
            rdoStringMode.Location = new Point(9, 29);
            rdoStringMode.Name = "rdoStringMode";
            rdoStringMode.Size = new Size(104, 24);
            rdoStringMode.TabIndex = 1;
            rdoStringMode.CheckedChanged += rdoStrategyPayloadString_CheckedChanged;
            rdoStringMode.TabStop = true;
            rdoStringMode.Text = "文本格式";
            rdoStringMode.UseVisualStyleBackColor = true;
            rdoStringMode.CheckedChanged += rdoStrategyPayloadString_CheckedChanged;
            // 
            // rdoHexMode
            // 
            rdoHexMode.Location = new Point(119, 29);
            rdoHexMode.Name = "rdoHexMode";
            rdoHexMode.Size = new Size(84, 24);
            rdoHexMode.TabIndex = 2;
            rdoHexMode.Text = "HEX格式";
            rdoHexMode.CheckedChanged += rdoStrategyPayloadHex_CheckedChanged;
            rdoHexMode.UseVisualStyleBackColor = true;
            rdoHexMode.CheckedChanged += rdoStrategyPayloadHex_CheckedChanged;
            // 
            // txtFullStrategyPayload
            // 
            txtFullStrategyPayload.Font = new Font("Consolas", 9F);
            txtFullStrategyPayload.Location = new Point(6, 55);
            txtFullStrategyPayload.Multiline = true;
            txtFullStrategyPayload.Name = "txtFullStrategyPayload";
            txtFullStrategyPayload.ReadOnly = true;
            txtFullStrategyPayload.ScrollBars = ScrollBars.Both;
            txtFullStrategyPayload.Size = new Size(442, 435);
            txtFullStrategyPayload.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 600);
            Controls.Add(tabControl1);
            Name = "MainForm";
            Text = "MQTT客户端演示程序";
            FormClosing += MainForm_FormClosing;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            grpConnectionSettings.ResumeLayout(false);
            grpConnectionSettings.PerformLayout();
            grpConnectionStatus.ResumeLayout(false);
            grpConnectionStatus.PerformLayout();
            grpWillMessage.ResumeLayout(false);
            grpWillMessage.PerformLayout();
            tabPage2.ResumeLayout(false);
            grpPublish.ResumeLayout(false);
            grpPublish.PerformLayout();
            grpSubscribe.ResumeLayout(false);
            grpSubscribe.PerformLayout();
            grpReceivedMessages.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            grpLog.ResumeLayout(false);
            grpLog.PerformLayout();
            tabPage4.ResumeLayout(false);
            grpStrategyList.ResumeLayout(false);
            grpStrategyDetail.ResumeLayout(false);
            tabStrategyDetail.ResumeLayout(false);
            tabPageStrategyInfo.ResumeLayout(false);
            tabPagePayload.ResumeLayout(false);
            tabPagePayload.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox grpConnectionSettings;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBrokerAddress;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.CheckBox chkCleanSession;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox grpConnectionStatus;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.GroupBox grpWillMessage;
        private System.Windows.Forms.CheckBox chkWillRetain;
        private System.Windows.Forms.ComboBox cmbWillQoS;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtWillMessage;
        private System.Windows.Forms.TextBox txtWillTopic;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkUseWill;
        private System.Windows.Forms.GroupBox grpPublish;
        private System.Windows.Forms.CheckBox chkRetain;
        private System.Windows.Forms.ComboBox cmbPublishQoS;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.TextBox txtPublishMessage;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtPublishTopic;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox grpSubscribe;
        private System.Windows.Forms.Button btnUnsubscribe;
        private System.Windows.Forms.ListBox lstSubscribedTopics;
        private System.Windows.Forms.ComboBox cmbSubscribeQoS;
        private System.Windows.Forms.Button btnSubscribe;
        private System.Windows.Forms.TextBox txtSubscribeTopic;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox grpReceivedMessages;
        private System.Windows.Forms.ListBox lstReceivedMessages;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.TextBox txtLog;
        private TabPage tabPage4;
        private System.Windows.Forms.RadioButton rdoPayloadString;
        private System.Windows.Forms.RadioButton rdoPayloadHex;
        private System.Windows.Forms.GroupBox grpStrategyList;
        private System.Windows.Forms.ListBox lstStrategies;
        private System.Windows.Forms.Button btnAddStrategy;
        private System.Windows.Forms.Button btnEditStrategy;
        private System.Windows.Forms.Button btnDeleteStrategy;
        private System.Windows.Forms.Button btnAddStrategyPlus;
        private System.Windows.Forms.Label payloadDisplayLabel;
        private System.Windows.Forms.RadioButton rdoStringMode;
        private System.Windows.Forms.RadioButton rdoHexMode;
        private System.Windows.Forms.GroupBox grpStrategyDetail;
        private System.Windows.Forms.Label labelStrategyDetail;
        private System.Windows.Forms.TextBox txtFullStrategyPayload;
        private System.Windows.Forms.TabControl tabStrategyDetail;
        private System.Windows.Forms.TabPage tabPageStrategyInfo;
        private System.Windows.Forms.TabPage tabPagePayload;
        private System.Windows.Forms.Label labelFormatSelector;
    }
}