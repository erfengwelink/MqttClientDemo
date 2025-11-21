// 添加MQTTnet所需的命名空间
using MQTTnet;
using MQTTnet.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Windows.Forms;

namespace MqttClientDemo
{
    public partial class MainForm : Form
    {
        private MQTTnet.Client.IMqttClient? mqttClient; // 使用正确的IMqttClient接口类型
        private CancellationTokenSource? cancellationTokenSource;

        // 定义payload打印方式的枚举
        private enum PayloadDisplayMode
        {
            String,
            Hex
        }

        // 默认使用字符串方式打印payload
        private PayloadDisplayMode currentPayloadMode = PayloadDisplayMode.String;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // 设置默认值
            txtBrokerAddress.Text = "test.mosquitto.org";
            txtPort.Text = "1883";
            txtClientId.Text = $"MQTTClient_{Guid.NewGuid().ToString()[..8]}";
            txtPublishTopic.Text = "mqtt_demo/test";
            txtSubscribeTopic.Text = "mqtt_demo/#";
            chkCleanSession.Checked = true;

            // 更新连接按钮状态
            UpdateConnectionButtonState();

            // 初始化策略配置页面控件
            InitializeStrategyConfigUI();
        }

        private void InitializeStrategyConfigUI()
    {
        // 确保策略详情相关控件可见并初始化
        if (labelStrategyDetail != null)
        {
            labelStrategyDetail.Visible = true;
            labelStrategyDetail.Text = "请选择或添加一个策略来查看或编辑详情";
        }
        
        // 确保策略负载格式选择相关控件初始化
        if (rdoStringMode != null)
        {
            rdoStringMode.Checked = true;
            AddLog("初始化: 策略格式选择设为文本格式");
        }
        
        if (rdoHexMode != null)
        {
            rdoHexMode.Checked = false;
            AddLog("初始化: 策略格式选择设为HEX格式(未选中)");
        }
        
        // 确保txtFullStrategyPayload控件初始化
        if (txtFullStrategyPayload != null)
        {
            txtFullStrategyPayload.Text = "请选择一个策略以查看详细报文内容";
            txtFullStrategyPayload.Refresh();
            AddLog("初始化: 策略报文显示区域设为提示文本");
        }

        // 初始化策略列表
        strategyList = new List<StrategyData>();
        UpdateStrategyList();
    }

        private void UpdateConnectionButtonState()
        {
            // 检查是否在UI线程上
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { UpdateConnectionButtonState(); });
                return;
            }

            // 使用null条件操作符和null合并操作符避免空引用
            if (btnConnect != null)
                btnConnect.Enabled = !string.IsNullOrWhiteSpace(txtBrokerAddress?.Text) &&
                                    !string.IsNullOrWhiteSpace(txtPort?.Text);
            if (btnDisconnect != null)
                btnDisconnect.Enabled = mqttClient != null;

            // 更新订阅、发布和取消订阅按钮状态
            if (btnSubscribe != null) btnSubscribe.Enabled = mqttClient != null;
            if (btnPublish != null) btnPublish.Enabled = mqttClient != null;
            if (btnUnsubscribe != null) btnUnsubscribe.Enabled = mqttClient != null;

            // 更新连接状态控件
            bool isConnected = mqttClient != null;
            if (grpConnectionStatus != null)
            {
                grpConnectionStatus.BackColor = isConnected ?
                    System.Drawing.Color.Green : System.Drawing.Color.Red;
            }
            if (lblConnectionStatus != null)
            {
                lblConnectionStatus.Text = isConnected ? "已连接" : "已断开";
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            ConnectToMqttBroker();
        }

        private async void ConnectToMqttBroker()
        {
            if (!int.TryParse(txtPort.Text, out int port))
            {
                ShowErrorMessage("请输入有效的端口号");
                return;
            }

            try
            {
                // 创建MQTT客户端实例
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                // 使用动态方式配置客户端选项，避免类型引用问题
                dynamic builder = new MqttClientOptionsBuilder();
                if (txtClientId != null && txtBrokerAddress != null && chkCleanSession != null)
                {
                    builder = builder.WithClientId(txtClientId.Text ?? "");
                    builder = builder.WithTcpServer(txtBrokerAddress.Text ?? "localhost", port);
                    builder = builder.WithCleanSession(chkCleanSession.Checked);
                }

                // 添加认证信息（如果提供）
                if ((txtUsername != null && !string.IsNullOrWhiteSpace(txtUsername.Text)) ||
                    (txtPassword != null && !string.IsNullOrWhiteSpace(txtPassword.Text)))
                {
                    builder = builder.WithCredentials(txtUsername?.Text ?? "", txtPassword?.Text ?? "");
                }

                // 添加遗嘱消息（如果启用）
                if (chkUseWill != null && chkUseWill.Checked && txtWillTopic != null && !string.IsNullOrWhiteSpace(txtWillTopic.Text))
                {
                    int selectedIndex = cmbWillQoS?.SelectedIndex ?? 0;
                    dynamic qosLevel = (MQTTnet.Protocol.MqttQualityOfServiceLevel)selectedIndex;
                    builder = builder.WithWillTopic(txtWillTopic.Text);
                    builder = builder.WithWillPayload(txtWillMessage?.Text ?? "");
                    builder = builder.WithWillQualityOfServiceLevel(qosLevel);
                    builder = builder.WithWillRetain(chkWillRetain?.Checked ?? false);
                }

                dynamic options = builder.Build();

                // 使用动态方式设置事件处理程序
                var mqttClientDynamic = (dynamic)mqttClient;
                mqttClientDynamic.ConnectedAsync += (Func<dynamic, Task>)((args) =>
                {
                    // 使用try-catch确保事件处理不会导致应用崩溃
                    try
                    {
                        // 安全地添加日志
                        if (this != null && !this.IsDisposed)
                        {
                            AddLog($"已连接到MQTT代理");
                            this.Invoke((MethodInvoker)delegate 
                            { 
                                if (!this.IsDisposed) UpdateConnectionButtonState(); 
                            });
                        }
                    }
                    catch { }
                    return Task.CompletedTask;
                });

                mqttClientDynamic.DisconnectedAsync += (Func<dynamic, Task>)((args) =>
                {
                    // 使用try-catch确保事件处理不会导致应用崩溃
                    try
                    {
                        // 安全地添加日志
                        if (this != null && !this.IsDisposed)
                        {
                            AddLog($"已断开连接");
                            this.Invoke((MethodInvoker)delegate 
                            { 
                                if (!this.IsDisposed) UpdateConnectionButtonState(); 
                            });
                        }
                    }
                    catch { }
                    return Task.CompletedTask;
                });

                // Before:
                // string payload = Encoding.UTF8.GetString((byte[])msg.PayloadSegment);

                // 使用正确的IMqttClient接口类型处理消息接收
                mqttClient.ApplicationMessageReceivedAsync += async args =>
                {
                    try
                    {
                        // 检查表单是否已销毁
                        if (this == null || this.IsDisposed)
                        {
                            return;
                        }

                        // 使用ToArray()方法将ArraySegment<byte>转换为byte[]
                        System.ArraySegment<byte> payloadSegment = args.ApplicationMessage.PayloadSegment;
                        //byte[] payloadBytes = payloadSegment.ToArray();
                        byte[] payloadBytes = new byte[payloadSegment.Count];
                        int payloadLength = payloadBytes.Length;

                        // 根据当前设置的打印方式格式化payload
                        string payload = FormatPayload(payloadBytes);

                        // 构建日志消息，实现主题后自动换行，包含payload长度
                        string messageText = $"[{DateTime.Now:HH:mm:ss}] topic: {args.ApplicationMessage.Topic}\r\n"
                                           + $"payload长度: {payloadLength} 字节\r\n"
                                           + $"payload: {payload}\r\n";

                        // 安全地添加日志
                        AddLog(messageText);

                        // 在UI线程上更新接收消息列表
                        try
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                // 再次检查表单是否已销毁
                                if (this.IsDisposed) return;

                                // 检查lstReceivedMessages控件是否存在且未被销毁
                                if (lstReceivedMessages != null && !lstReceivedMessages.IsDisposed)
                                {
                                    lstReceivedMessages.Items.Add($"[{DateTime.Now:HH:mm:ss}] topic: {args.ApplicationMessage.Topic}");
                                    lstReceivedMessages.Items.Add($"payload长度: {payloadLength} 字节");
                                    lstReceivedMessages.Items.Add($"payload: {payload}");
                                    lstReceivedMessages.Items.Add(""); // 添加空行分隔不同消息
                                    // 自动滚动到最新消息
                                    lstReceivedMessages.TopIndex = lstReceivedMessages.Items.Count - 1;
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            AddLog($"更新接收消息列表时出错: {ex.Message}");
                        }

                    }
                    catch (Exception ex)
                    {
                        AddLog($"处理接收到的消息时出错: {ex.Message}");
                    }
                    await Task.CompletedTask;
                };

                // 在MQTTnet 4.3.2.930版本中，MqttClient类没有ReconnectingAsync事件
                // 移除对该事件的引用以避免运行时错误

                // 开始连接
                AddLog("正在连接到MQTT代理..." + (txtBrokerAddress?.Text ?? "未知地址"));
                await mqttClientDynamic.ConnectAsync(options, CancellationToken.None);

                // 启动保持连接的任务
                cancellationTokenSource = new CancellationTokenSource();
                // 使用await确保任务正确执行
                _ = Task.Run(() => KeepConnectionAlive(cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "连接MQTT代理失败");
                AddLog($"连接失败: {ex.Message}");
                ShowErrorMessage($"连接失败: {ex.Message}");
            }
        }

        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (mqttClient != null)
            {
                try
                {
                    AddLog("正在断开连接...");
                    cancellationTokenSource?.Cancel();
                    // 在MQTTnet 4.3.2.930版本中，DisconnectAsync方法需要参数
                    // 创建断开连接选项
                    var disconnectOptions = new MQTTnet.Client.MqttClientDisconnectOptions();
                    await mqttClient.DisconnectAsync(disconnectOptions);

                    // 断开连接成功后将mqttClient设置为null
                    mqttClient = null;
                    AddLog("已成功断开连接");
                    // 清空已订阅主题列表
                    lstSubscribedTopics?.Items?.Clear();
                    // 更新连接状态控件
                    UpdateConnectionButtonState();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "断开连接失败");
                    AddLog($"断开连接失败: {ex.Message}");
                }
            }
        }

        private async void btnSubscribe_Click(object sender, EventArgs e)
        {
            if (mqttClient == null)
            {
                ShowErrorMessage("请先连接到MQTT代理");
                return;
            }

            string topic = txtSubscribeTopic?.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(topic))
            {
                ShowErrorMessage("请输入要订阅的主题");
                return;
            }

            try
            {
                // 使用完全限定名确保类型正确
                int subscribeQoSIndex = cmbSubscribeQoS?.SelectedIndex ?? 0;
                var qos = (MQTTnet.Protocol.MqttQualityOfServiceLevel)subscribeQoSIndex; // 已添加null检查
                // 使用动态方式订阅主题
                dynamic mqttClientDynamic = mqttClient;
                dynamic factory = new MqttFactory();
                dynamic subscribeOptionsBuilder = factory.CreateSubscribeOptionsBuilder();

                // 直接传递参数给WithTopicFilter方法，匹配MQTTnet 4.3.2.930版本的方法签名
                dynamic optionsBuilderWithFilter = subscribeOptionsBuilder.WithTopicFilter(
                    topic,  // 主题
                    qos,    // QoS级别
                    false,  // NoLocal
                    false,  // RetainAsPublished
                    0       // RetainHandling
                );
                dynamic subscribeOptions = optionsBuilderWithFilter.Build();
                await mqttClientDynamic.SubscribeAsync(subscribeOptions);
                AddLog($"已订阅主题: {topic}, QoS: {qos}");

                // 将订阅的主题添加到列表
                if (lstSubscribedTopics != null && !lstSubscribedTopics.Items.Contains(topic))
                {
                    lstSubscribedTopics.Items.Add(topic);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "订阅主题失败");
                AddLog($"订阅失败: {ex.Message}");
                ShowErrorMessage($"订阅失败: {ex.Message}");
            }
        }

        private async void btnPublish_Click(object sender, EventArgs e)
        {
            if (mqttClient == null)
            {
                ShowErrorMessage("请先连接到MQTT代理");
                return;
            }

            string topic = txtPublishTopic?.Text?.Trim() ?? "";
            string message = txtPublishMessage?.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(topic))
            {
                ShowErrorMessage("请输入要发布的主题");
                return;
            }

            try
            {
                // 使用完全限定名确保类型正确
                int publishQoSIndex = cmbPublishQoS?.SelectedIndex ?? 0;
                var qos = (MQTTnet.Protocol.MqttQualityOfServiceLevel)publishQoSIndex; // 添加null检查
                // 使用动态方式发布消息
                dynamic mqttClientDynamic = mqttClient;
                dynamic messageBuilder = new MqttApplicationMessageBuilder();
                messageBuilder = messageBuilder.WithTopic(topic);
                messageBuilder = messageBuilder.WithPayload(Encoding.UTF8.GetBytes(message));
                messageBuilder = messageBuilder.WithQualityOfServiceLevel(qos);
                messageBuilder = messageBuilder.WithRetainFlag(chkRetain?.Checked ?? false);
                dynamic mqttMessage = messageBuilder.Build();
                await mqttClientDynamic.PublishAsync(mqttMessage);
                AddLog($"已发布消息到主题: {topic}, QoS: {qos}, 保留: {chkRetain?.Checked ?? false}");
                AddLog($"消息内容: {message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "发布消息失败");
                AddLog($"发布失败: {ex.Message}");
                ShowErrorMessage($"发布失败: {ex.Message}");
            }
        }

        private async void btnUnsubscribe_Click(object sender, EventArgs e)
        {
            if (mqttClient == null)
            {
                ShowErrorMessage("请先连接到MQTT代理");
                return;
            }

            if (lstSubscribedTopics == null || lstSubscribedTopics.SelectedItem == null)
            {
                ShowErrorMessage("请选择要取消订阅的主题");
                return;
            }

            string topic = lstSubscribedTopics.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(topic))
            {
                ShowErrorMessage("无效的主题");
                return;
            }

            try
            {
                // 使用动态方式取消订阅
                dynamic mqttClientDynamic = mqttClient;
                await mqttClientDynamic.UnsubscribeAsync(topic);
                AddLog($"已取消订阅主题: {topic}");
                lstSubscribedTopics.Items?.Remove(topic); // 添加null条件操作符
            }
            catch (Exception ex)
            {
                Log.Error(ex, "取消订阅失败");
                AddLog($"取消订阅失败: {ex.Message}");
                ShowErrorMessage($"取消订阅失败: {ex.Message}");
            }
        }

        private async Task KeepConnectionAlive(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (mqttClient != null)
                    {
                        try
                        {
                            // 使用动态方式发送ping
                            dynamic mqttClientDynamic = mqttClient;
                            await mqttClientDynamic.PingAsync(cancellationToken);
                        }
                        catch { }
                    }
                    // 每30秒发送一次ping
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 预期的取消操作，无需记录
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保持连接任务失败");
            }
        }

        private void AddLog(string message)
        {
            // 确保message不为null
            message = message ?? string.Empty;

            // 添加检查确保txtLog控件存在且未被销毁
            if (txtLog != null && !txtLog.IsDisposed)
            {
                if (txtLog.InvokeRequired)
                {
                    try
                    {
                        txtLog.Invoke(new Action<string>(AddLog), message);
                    }
                    catch (ObjectDisposedException)
                    {
                        // 控件已被销毁，忽略异常
                    }
                }
                else
                {
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                    txtLog.ScrollToCaret();
                }
            }
        }

        private void ShowErrorMessage(string message)
        {
            // 确保message不为null
            string safeMessage = message ?? "未知错误";
            MessageBox.Show(safeMessage, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AddLog("错误: " + safeMessage);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 取消任何正在运行的任务
            cancellationTokenSource?.Cancel();
            
            // 安全地清理MQTT客户端
            if (mqttClient != null)
            {
                try
                {
                    // 注意：无法直接将事件设为null，事件只能通过+=或-=操作符来添加或移除处理程序
                    // 后续将mqttClient设置为null，这已足够防止后续引用和内存泄漏
                    
                    // 使用更安全的方式断开连接
                    var disconnectOptions = new MQTTnet.Client.MqttClientDisconnectOptions();
                    // 不使用Wait()以避免死锁，改为非阻塞方式
                    _ = mqttClient.DisconnectAsync(disconnectOptions);
                }
                catch { }
                finally
                {
                    // 确保将mqttClient设置为null，避免后续引用
                    mqttClient = null;
                }
            }
        }

        private void txtBrokerAddress_TextChanged(object sender, EventArgs e)
        {
            UpdateConnectionButtonState();
        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            UpdateConnectionButtonState();
        }

        private void grpConnectionStatus_Enter(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 根据设置的显示模式格式化payload
        /// </summary>
        /// <param name="payloadBytes">payload的字节数组</param>
        /// <returns>格式化后的payload字符串</returns>
        private string FormatPayload(byte[] payloadBytes)
        {
            if (payloadBytes.Length == 0)
            {
                return "(空消息)";
            }

            switch (currentPayloadMode)
            {
                case PayloadDisplayMode.Hex:
                    // 以十六进制格式显示
                    return BitConverter.ToString(payloadBytes).Replace("-", " ");
                case PayloadDisplayMode.String:
                default:
                    try
                    {
                        // 尝试以UTF-8字符串格式显示
                        return Encoding.UTF8.GetString(payloadBytes);
                    }
                    catch
                    {
                        // 如果无法转换为字符串，则显示为十六进制
                        return BitConverter.ToString(payloadBytes).Replace("-", " ");
                    }
            }
        }

        private void rdoStringMode_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                currentPayloadMode = PayloadDisplayMode.String;
                AddLog("切换到字符串格式显示Payload");
            }
        }

        private void rdoHexMode_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                currentPayloadMode = PayloadDisplayMode.Hex;
                AddLog("切换到十六进制格式显示Payload");
            }
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {

        }

        private void payloadDisplayLabel_Click(object sender, EventArgs e)
        {

        }

        // 复制策略配置报文菜单项点击事件
        private void CopyPayloadMenuItem_Click(object? sender, EventArgs e)
        {
            if (txtFullStrategyPayload != null && !string.IsNullOrEmpty(txtFullStrategyPayload.Text))
            {
                Clipboard.SetText(txtFullStrategyPayload.Text);
            }
        }

        // 策略列表相关功能

        private List<StrategyData> strategyList = new List<StrategyData>();

        private void btnAddStrategy_Click(object sender, EventArgs e)
        {
            // 显示策略配置窗口
            using (StrategyConfigForm configForm = new StrategyConfigForm())
            {
                if (configForm.ShowDialog() == DialogResult.OK && configForm.StrategyData != null)
                {
                    // 保存新策略并更新列表
                    strategyList?.Add(configForm.StrategyData);
                    UpdateStrategyList();
                    AddLog($"已添加新策略: 0x{configForm.StrategyData.StrategyId:X8}");
                }
            }
        }

        private void btnEditStrategy_Click(object sender, EventArgs e)
        {
            if (lstStrategies != null && lstStrategies.SelectedIndex >= 0)
            {
                StrategyData? selectedStrategy = lstStrategies.SelectedIndex >= 0 && lstStrategies.SelectedIndex < strategyList.Count ? strategyList[lstStrategies.SelectedIndex] : null; // 安全索引访问
                if (selectedStrategy == null)
                {
                    ShowErrorMessage("选中的策略无效");
                    return;
                }
                using (StrategyConfigForm configForm = new StrategyConfigForm(selectedStrategy))
                {
                    if (configForm.ShowDialog() == DialogResult.OK)
                    {
                        // 更新策略并刷新列表
                        if (lstStrategies.SelectedIndex >= 0 && lstStrategies.SelectedIndex < strategyList.Count && configForm != null && configForm.StrategyData != null) // 添加安全检查
                        {
                            strategyList[lstStrategies.SelectedIndex] = configForm.StrategyData;
                            UpdateStrategyList();
                            AddLog($"已更新策略: 0x{configForm.StrategyData.StrategyId:X8}");
                        }
                    }
                }
            }
            else
            {
                ShowErrorMessage("请先选择要编辑的策略");
            }
        }

        private void btnDeleteStrategy_Click(object sender, EventArgs e)
        {
            if (lstStrategies != null && lstStrategies.SelectedIndex >= 0)
            {
                StrategyData? selectedStrategy = lstStrategies.SelectedIndex >= 0 && lstStrategies.SelectedIndex < strategyList.Count ? strategyList[lstStrategies.SelectedIndex] : null; // 安全索引访问
                if (selectedStrategy != null && MessageBox.Show($"确定要删除策略 0x{selectedStrategy.StrategyId:X8} 吗？",
                    "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // 删除策略并更新列表
                    if (lstStrategies.SelectedIndex >= 0 && lstStrategies.SelectedIndex < strategyList.Count) // 添加索引有效性检查
                    {
                        strategyList.RemoveAt(lstStrategies.SelectedIndex);
                        UpdateStrategyList();
                        AddLog($"已删除策略: 0x{selectedStrategy.StrategyId:X8}");
                    }
                }
            }
            else
            {
                ShowErrorMessage("请先选择要删除的策略");
            }
        }

        // 策略选中事件
        private void lstStrategies_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // 确保在UI线程上执行
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler(lstStrategies_SelectedIndexChanged), sender!, e);
                return;
            }
            
            try
            {
                AddLog("策略列表选中事件触发");
                
                // 验证事件参数
                if (e == null)
                {
                    AddLog("警告: 事件参数为空");
                    return;
                }
                
                // 根据选中状态启用/禁用按钮
                if (btnEditStrategy != null)
                {
                    btnEditStrategy.Enabled = lstStrategies?.SelectedIndex != -1;
                }
                if (btnDeleteStrategy != null)
                {
                    btnDeleteStrategy.Enabled = lstStrategies?.SelectedIndex != -1;
                }

                // 检查控件和索引有效性
                if (lstStrategies == null || lstStrategies.SelectedIndex == -1)
                {
                    AddLog("未选中任何策略");
                    DisplayStrategyDetail(null);
                    return;
                }
                
                // 安全检查策略列表和索引范围
                if (strategyList == null || lstStrategies.SelectedIndex >= strategyList.Count)
                {
                    AddLog($"警告: 无效的策略索引 {lstStrategies.SelectedIndex}");
                    DisplayStrategyDetail(null);
                    return;
                }
                
                // 获取并显示选中的策略详情
                StrategyData selectedStrategy = strategyList[lstStrategies.SelectedIndex];
                if (selectedStrategy != null)
                {
                    AddLog($"选中策略索引: {lstStrategies.SelectedIndex}, 策略ID: 0x{selectedStrategy.StrategyId:X8}");
                    DisplayStrategyDetail(selectedStrategy);
                }
                else
                {
                    AddLog($"警告: 选中索引 {lstStrategies.SelectedIndex} 的策略数据为空");
                    DisplayStrategyDetail(null);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"处理策略选中事件时发生错误: {ex.Message}\n{ex.StackTrace}";
                AddLog(errorMsg);
                // 出错时清空详情显示
                DisplayStrategyDetail(null);
            }
        }

        // 显示策略详细信息
        private void DisplayStrategyDetail(StrategyData? strategy)
        {
            // 记录方法调用，帮助调试
            AddLog("DisplayStrategyDetail方法被调用");
            
            // 确保在UI线程上执行
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<StrategyData?>(DisplayStrategyDetail), strategy);
                return;
            }

            try
            {
                // 检查并初始化控件属性
                EnsureControlsInitialized();

                // 添加null检查，防止空引用异常
                if (strategy == null)
                {
                    AddLog("警告: 传入的策略数据为空");
                    if (labelStrategyDetail != null)
                    {
                        labelStrategyDetail.Text = "暂无策略详情信息";
                        labelStrategyDetail.Refresh();
                    }
                    // 调用UpdateStrategyPayloadDisplay来处理策略报文显示
                    UpdateStrategyPayloadDisplay(null);
                    return;
                }

                AddLog($"正在显示策略详情: 0x{strategy.StrategyId:X8}");

                StringBuilder detailText = new StringBuilder();

                // 基本信息
                detailText.AppendLine($"策略ID: 0x{strategy.StrategyId:X8}");
                string statusText = (strategy.EnabledFlag == 1) ? "启用" : "禁用";
                detailText.AppendLine($"状态: {statusText}");
                detailText.AppendLine();

                // 日期时间配置 - 更灵活地处理日期和时间显示
                detailText.AppendLine("日期范围:");
                detailText.AppendLine($"  开始: {FormatBcdDateToString(strategy.StartDate)}");
                detailText.AppendLine($"  结束: {FormatBcdDateToString(strategy.EndDate)}");

                detailText.AppendLine("时间范围:");
                detailText.AppendLine($"  开始: {FormatBcdTimeToString(strategy.StartTime)}");
                detailText.AppendLine($"  结束: {FormatBcdTimeToString(strategy.EndTime)}");
                detailText.AppendLine();

                // 星期-节假日策略
                detailText.AppendLine("星期-节假日策略:");
                detailText.AppendLine($"  0x{strategy.WeekHolidayData:X4}");
                detailText.AppendLine();

                // 条件信息
                detailText.AppendLine("触发条件:");
                if (strategy.TriggerConditions != null && strategy.TriggerConditions.Count > 0)
                {
                    foreach (var condition in strategy.TriggerConditions)
                    {
                        if (condition != null)
                        {
                            detailText.AppendLine($"  - {condition}");
                        }
                    }
                }
                else
                {
                    detailText.AppendLine("  无触发条件");
                }
                detailText.AppendLine();

                detailText.AppendLine("状态条件:");
                if (strategy.StatusConditions != null && strategy.StatusConditions.Count > 0)
                {
                    foreach (var condition in strategy.StatusConditions)
                    {
                        if (condition != null)
                        {
                            detailText.AppendLine($"  - {condition}");
                        }
                    }
                }
                else
                {
                    detailText.AppendLine("  无状态条件");
                }
                detailText.AppendLine();

                detailText.AppendLine("动作条件:");
                if (strategy.ActionConditions != null && strategy.ActionConditions.Count > 0)
                {
                    foreach (var condition in strategy.ActionConditions)
                    {
                        if (condition != null)
                        {
                            detailText.AppendLine($"  - {condition}");
                        }
                    }
                }
                else
                {
                    detailText.AppendLine("  无动作条件");
                }

                // 更新策略详情标签
                string finalDetailText = detailText.ToString();
                AddLog($"生成的策略详情文本长度: {finalDetailText.Length} 字符");
                
                if (labelStrategyDetail != null)
                {
                    labelStrategyDetail.Text = finalDetailText;
                    labelStrategyDetail.Refresh();
                    AddLog("已更新labelStrategyDetail控件文本");
                }
                else
                {
                    AddLog("错误: labelStrategyDetail控件为空");
                }

                // 更新完整策略报文显示
                UpdateStrategyPayloadDisplay(strategy);
                
                // 强制刷新txtFullStrategyPayload控件，确保内容显示
                if (txtFullStrategyPayload != null)
                {
                    txtFullStrategyPayload.Refresh();
                    AddLog("已强制刷新txtFullStrategyPayload控件");
                }
                
                // 确保tabStrategyDetail和选项卡可见并刷新
                if (tabStrategyDetail != null && !tabStrategyDetail.IsDisposed)
                {
                    tabStrategyDetail.Visible = true;
                    tabStrategyDetail.Refresh();
                    AddLog("已刷新并确保tabStrategyDetail可见");
                }
                
                // 强制UI更新
                this.Refresh();
                Application.DoEvents(); // 处理所有待处理的UI事件
            }
            catch (Exception ex)
            {
                // 记录异常信息
                string errorMsg = $"显示策略详情时发生错误: {ex.Message}\n{ex.StackTrace}";
                AddLog(errorMsg);
                if (labelStrategyDetail != null)
                {
                    labelStrategyDetail.Text = $"显示策略详情时发生错误: {ex.Message}";
                    labelStrategyDetail.Refresh();
                }
            }
        }
        
        // 确保控件已初始化并可见
        private void EnsureControlsInitialized()
        {
            // 确保策略详情相关控件可见并初始化
            if (labelStrategyDetail != null)
            {
                labelStrategyDetail.Visible = true;
                labelStrategyDetail.AutoSize = true; // 允许标签自动调整大小
                AddLog("已初始化labelStrategyDetail控件");
            }
            else
            {
                AddLog("警告: labelStrategyDetail控件为空");
            }

            // 确保TabControl相关控件可见并初始化
            if (tabStrategyDetail != null)
            {
                tabStrategyDetail.Visible = true;
                AddLog("已初始化tabStrategyDetail控件");
            }
            else
            {
                AddLog("警告: tabStrategyDetail控件为空");
            }
            
            // 确保格式选择器控件初始化（使用正确的策略报文格式控件）
            if (rdoStringMode != null)
            {
                rdoStringMode.Checked = true; // 默认选择文本格式
                AddLog("已初始化rdoStringMode控件");
            }
            else
            {
                AddLog("警告: rdoStringMode控件为空");
            }
            
            if (txtFullStrategyPayload != null)
            {
                txtFullStrategyPayload.Visible = true;
                txtFullStrategyPayload.Multiline = true;
                txtFullStrategyPayload.ScrollBars = ScrollBars.Both;
                txtFullStrategyPayload.ReadOnly = true;
                txtFullStrategyPayload.WordWrap = true; // 启用自动换行
                txtFullStrategyPayload.AutoSize = false; // 确保不会自动调整大小影响布局
                txtFullStrategyPayload.Enabled = true; // 确保控件可用
                txtFullStrategyPayload.Refresh(); // 强制刷新
                AddLog("已初始化txtFullStrategyPayload控件");
            }
            else
            {
                AddLog("警告: txtFullStrategyPayload控件为空");
            }
        }

        // 更新策略配置报文显示
        private void UpdateStrategyPayloadDisplay(StrategyData? strategy)
        {
            try
            {
                if (txtFullStrategyPayload != null && strategy != null)
                {
                    // 记录当前格式选择状态
                    AddLog($"当前格式选择 - rdoStringMode状态: {(rdoStringMode?.Checked ?? false)}, rdoHexMode状态: {(rdoHexMode?.Checked ?? false)}");
                    
                    // 优先检查HEX模式
                    if (rdoHexMode != null && rdoHexMode.Checked)
                    {
                        AddLog("使用HEX格式显示策略报文");
                        string hexPayload = GenerateHexStrategyPayload(strategy);
                        AddLog($"生成的HEX格式策略报文长度: {hexPayload.Length} 字符");
                        txtFullStrategyPayload.Text = hexPayload;
                    }
                    // 然后检查文本模式
                    else if (rdoStringMode != null && rdoStringMode.Checked)
                    {
                        AddLog("使用文本格式显示策略报文");
                        string payload = GenerateFullStrategyPayload(strategy);
                        AddLog($"生成的文本格式策略报文长度: {payload.Length} 字符");
                        txtFullStrategyPayload.Text = payload;
                    }
                    else
                    {
                        AddLog("未检测到有效格式选择，默认使用文本格式");
                        // 默认使用文本格式
                        string payload = GenerateFullStrategyPayload(strategy);
                        txtFullStrategyPayload.Text = payload;
                    }
                    
                    // 强制刷新以确保显示更新
                    txtFullStrategyPayload.Refresh();
                    txtFullStrategyPayload.Select(0, 0); // 重置光标位置
                    AddLog("已更新txtFullStrategyPayload控件文本并强制刷新");
                }
                else if (txtFullStrategyPayload != null)
                {
                    txtFullStrategyPayload.Text = "请选择一个策略以查看详细报文内容";
                    txtFullStrategyPayload.Refresh();
                    AddLog("策略为空，更新为提示文本");
                }
                else
                {
                    AddLog("错误: txtFullStrategyPayload控件为空");
                }
            }
            catch (Exception ex)
            {
                AddLog($"更新策略报文显示错误: {ex.Message}");
            }
        }

        // 生成HEX格式的策略配置报文
        private string GenerateHexStrategyPayload(StrategyData? strategy)
        {
            try
            {
                if (strategy == null)
                {
                    return "策略数据为空";
                }

                StringBuilder hexBuilder = new StringBuilder();
                StringBuilder hexStr = new StringBuilder();


                // 添加策略ID (4字节)
                hexBuilder.AppendLine("=== 策略ID (4字节) ===");
                hexBuilder.AppendLine($"{strategy.StrategyId:X8}");
                hexStr.AppendLine($"{strategy.StrategyId:X8}");

                // 添加使能标志 (1字节)
                hexBuilder.AppendLine("\n=== 使能标志 (1字节) ===");
                hexBuilder.AppendLine($"{strategy.EnabledFlag:X2}");
                hexStr.AppendLine($"{strategy.EnabledFlag:X2}");


                // 添加日期数据 (每个日期2字节)
                hexBuilder.AppendLine("\n=== 日期数据 (每个2字节) ===");
                if (strategy.StartDate != null)
                {
                    hexBuilder.AppendLine($"开始日期: {BitConverter.ToString(strategy.StartDate).Replace("-", " ")}");
                }
                if (strategy.EndDate != null)
                {
                    hexBuilder.AppendLine($"结束日期: {BitConverter.ToHexString(strategy.EndDate).Replace("-", " ")}");
                }
                
                // 添加时间数据 (每个时间2字节)
                hexBuilder.AppendLine("\n=== 时间数据 (每个2字节) ===");
                if (strategy.StartTime != null)
                {
                    hexBuilder.AppendLine($"开始时间: {BitConverter.ToString(strategy.StartTime).Replace("-", " ")}");
                }
                if (strategy.EndTime != null)
                {
                    hexBuilder.AppendLine($"结束时间: {BitConverter.ToString(strategy.EndTime).Replace("-", " ")}");
                }
                
                // 添加星期-节假日数据 (2字节)
                hexBuilder.AppendLine("\n=== 星期-节假日数据 (2字节) ===");
                hexBuilder.AppendLine($"{strategy.WeekHolidayData:X4}");
                
                // 添加触发条件数量和数据
                hexBuilder.AppendLine("\n=== 触发条件 ===");
                if (strategy.TriggerConditions != null)
                {
                    hexBuilder.AppendLine($"触发条件数量: {strategy.TriggerConditions.Count:X2}");
                    for (int i = 0; i < strategy.TriggerConditions.Count; i++)
                    {
                        string condition = (strategy.TriggerConditions[i]?.ToString() ?? "");
                        byte[] conditionBytes = System.Text.Encoding.ASCII.GetBytes(condition);
                        hexBuilder.AppendLine($"条件{i+1} (HEX): {BitConverter.ToString(conditionBytes).Replace("-", " ")}");
                    }
                }
                else
                {
                    hexBuilder.AppendLine("触发条件数量: 00");
                }
                
                // 添加状态条件
                hexBuilder.AppendLine("\n=== 状态条件 ===");
                if (strategy.StatusConditions != null)
                {
                    hexBuilder.AppendLine($"状态条件数量: {strategy.StatusConditions.Count:X2}");
                    for (int i = 0; i < strategy.StatusConditions.Count; i++)
                    {
                        string condition = (strategy.StatusConditions[i]?.ToString() ?? "");
                        byte[] conditionBytes = System.Text.Encoding.ASCII.GetBytes(condition);
                        hexBuilder.AppendLine($"状态{i+1} (HEX): {BitConverter.ToString(conditionBytes).Replace("-", " ")}");
                    }
                }
                else
                {
                    hexBuilder.AppendLine("状态条件数量: 00");
                }
                
                // 添加动作条件
                hexBuilder.AppendLine("\n=== 动作条件 ===");
                if (strategy.ActionConditions != null)
                {
                    hexBuilder.AppendLine($"动作条件数量: {strategy.ActionConditions.Count:X2}");
                    for (int i = 0; i < strategy.ActionConditions.Count; i++)
                    {
                        string condition = (strategy.ActionConditions[i]?.ToString() ?? "");
                        byte[] conditionBytes = System.Text.Encoding.ASCII.GetBytes(condition);
                        hexBuilder.AppendLine($"动作{i+1} (HEX): {BitConverter.ToString(conditionBytes).Replace("-", " ")}");
                    }
                }
                else
                {
                    hexBuilder.AppendLine("动作条件数量: 00");
                }
                
                return hexBuilder.ToString();
            }
            catch (Exception ex)
            {
                AddLog($"生成HEX策略报文错误: {ex.Message}");
                return $"生成HEX格式错误: {ex.Message}";
            }
        }

        // 处理策略配置报文格式选择切换事件
        private void rdoStrategyPayloadString_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                HandleStrategyPayloadFormatChange();
            }
        }

        private void rdoStrategyPayloadHex_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                HandleStrategyPayloadFormatChange();
            }
        }

        // 处理格式选择切换的核心逻辑
        private void HandleStrategyPayloadFormatChange()
        {
            try
            {
                // 获取当前选中的策略
                StrategyData? currentStrategy = null;
                
                // 直接通过索引从strategyList获取选中的策略，与lstStrategies_SelectedIndexChanged方法保持一致
                if (lstStrategies != null && lstStrategies.SelectedIndex != -1 && strategyList != null && lstStrategies.SelectedIndex < strategyList.Count)
                {
                    currentStrategy = strategyList[lstStrategies.SelectedIndex];
                    AddLog($"通过索引直接获取选中策略: 0x{currentStrategy.StrategyId:X8}");
                }
                else if (lstStrategies != null && lstStrategies.SelectedIndex != -1)
                {
                    // 索引无效时的错误处理
                    AddLog($"警告: 无效的策略索引 {lstStrategies.SelectedIndex}");
                }
                else
                {
                    AddLog("未选中任何策略");
                }
                
                // 更新显示
                UpdateStrategyPayloadDisplay(currentStrategy);
            }
            catch (Exception ex)
            {
                AddLog($"格式切换错误: {ex.Message}");
            }
        }

        // 生成完整策略配置报文
        private string GenerateFullStrategyPayload(StrategyData? strategy)
        {
            try
            {
                AddLog("开始生成完整策略报文");
                
                if (strategy == null)
                {
                    AddLog("警告: 传入的策略数据为空");
                    return "策略数据为空";
                }

                AddLog($"生成策略ID: 0x{strategy.StrategyId:X8} 的完整报文");
                
                StringBuilder payloadBuilder = new StringBuilder();

                // 1. 通用属性
                payloadBuilder.AppendLine("=== 通用属性 ===");
                payloadBuilder.AppendLine($"策略ID: 0x{strategy.StrategyId:X8}");
                string status = (strategy.EnabledFlag == 1) ? "启用" : "禁用";
                payloadBuilder.AppendLine($"状态: {status}");
                payloadBuilder.AppendLine($"使能标志: {strategy.EnabledFlag}");

                // 显示日期时间信息，无论长度如何都尝试格式化
                payloadBuilder.AppendLine($"开始日期: {FormatBcdDateToString(strategy.StartDate)}");
                payloadBuilder.AppendLine($"结束日期: {FormatBcdDateToString(strategy.EndDate)}");
                payloadBuilder.AppendLine($"开始时间: {FormatBcdTimeToString(strategy.StartTime)}");
                payloadBuilder.AppendLine($"结束时间: {FormatBcdTimeToString(strategy.EndTime)}");
                payloadBuilder.AppendLine($"星期-节假日策略: 0x{strategy.WeekHolidayData:X4}");

                // 添加星期-节假日策略的详细说明
                string weekHolidayText = FormatWeekHolidayData(strategy.WeekHolidayData);
                payloadBuilder.AppendLine($"星期-节假日详细说明: {weekHolidayText}");
                payloadBuilder.AppendLine();

                // 2. 触发条件
                payloadBuilder.AppendLine("=== 触发条件 ===");
                if (strategy.TriggerConditions != null && strategy.TriggerConditions.Count > 0)
                {
                    payloadBuilder.AppendLine($"触发条件数量: {strategy.TriggerConditions.Count}");
                    for (int i = 0; i < strategy.TriggerConditions.Count; i++)
                    {
                        var condition = strategy.TriggerConditions[i];
                        if (condition != null)
                        {
                            payloadBuilder.AppendLine($"条件{i + 1}: {condition}");
                        }
                        else
                        {
                            payloadBuilder.AppendLine($"条件{i + 1}: 无效条件(空)");
                        }
                    }
                }
                else
                {
                    payloadBuilder.AppendLine("无触发条件");
                }
                payloadBuilder.AppendLine();

                // 3. 状态条件
                payloadBuilder.AppendLine("=== 状态条件 ===");
                if (strategy.StatusConditions != null && strategy.StatusConditions.Count > 0)
                {
                    payloadBuilder.AppendLine($"状态条件数量: {strategy.StatusConditions.Count}");
                    for (int i = 0; i < strategy.StatusConditions.Count; i++)
                    {
                        var condition = strategy.StatusConditions[i];
                        if (condition != null)
                        {
                            payloadBuilder.AppendLine($"条件{i + 1}: {condition}");
                        }
                        else
                        {
                            payloadBuilder.AppendLine($"条件{i + 1}: 无效条件(空)");
                        }
                    }
                }
                else
                {
                    payloadBuilder.AppendLine("无状态条件");
                }
                payloadBuilder.AppendLine();

                // 4. 动作执行
                payloadBuilder.AppendLine("=== 动作执行 ===");
                if (strategy.ActionConditions != null && strategy.ActionConditions.Count > 0)
                {
                    payloadBuilder.AppendLine($"动作条件数量: {strategy.ActionConditions.Count}");
                    for (int i = 0; i < strategy.ActionConditions.Count; i++)
                    {
                        var condition = strategy.ActionConditions[i];
                        if (condition != null)
                        {
                            payloadBuilder.AppendLine($"动作{i + 1}: {condition}");
                        }
                        else
                        {
                            payloadBuilder.AppendLine($"动作{i + 1}: 无效动作(空)");
                        }
                    }
                }
                else
                {
                    payloadBuilder.AppendLine("无动作条件");
                }

                // 添加生成时间戳
                payloadBuilder.AppendLine();
                payloadBuilder.AppendLine($"=== 生成信息 ===");
                payloadBuilder.AppendLine($"生成时间: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                
                string finalPayload = payloadBuilder.ToString();
                AddLog($"策略报文生成完成，长度: {finalPayload.Length} 字符");
                
                return finalPayload;
            }
            catch (Exception ex)
            {
                string errorMsg = $"生成策略报文时发生错误: {ex.Message}\n{ex.StackTrace}";
                AddLog(errorMsg);
                return errorMsg; // 返回详细错误信息以便调试
            }
        }

        // 格式化星期-节假日数据的静态方法
        private string FormatWeekHolidayData(ushort weekHolidayData)
        {
            StringBuilder sb = new StringBuilder();

            // 星期模式
            if ((weekHolidayData & 0x80) != 0)
            {
                sb.Append("星期模式: ");
                if ((weekHolidayData & 0x01) != 0) sb.Append("周日 ");
                if ((weekHolidayData & 0x02) != 0) sb.Append("周一 ");
                if ((weekHolidayData & 0x04) != 0) sb.Append("周二 ");
                if ((weekHolidayData & 0x08) != 0) sb.Append("周三 ");
                if ((weekHolidayData & 0x10) != 0) sb.Append("周四 ");
                if ((weekHolidayData & 0x20) != 0) sb.Append("周五 ");
                if ((weekHolidayData & 0x40) != 0) sb.Append("周六 ");
            }
            else
            {
                sb.Append("未启用星期模式 ");
            }

            // 节假日模式
            if ((weekHolidayData & 0x100) != 0)
            {
                sb.Append("| 节假日模式: ");
                sb.Append((weekHolidayData & 0x200) != 0 ? "法定节假日" : "工作日");
            }
            else
            {
                sb.Append("| 未启用节假日模式");
            }

            // 逻辑关系
            sb.Append(" | 逻辑关系: ");
            sb.Append((weekHolidayData & 0x400) != 0 ? "与" : "或");

            return sb.ToString();
        }

        // 格式化BCD日期为字符串
        // 格式化BCD时间为字符串
        private string FormatBcdTimeToString(byte[]? bcdTime)
        {
            if (bcdTime == null)
                return "无效时间(空)";

            try
            {
                // 如果长度不是2，尝试处理现有数据
                int hour = 0;
                int minute = 0;

                if (bcdTime.Length >= 1)
                {
                    hour = ((bcdTime[0] >> 4) * 10) + (bcdTime[0] & 0x0F);
                }
                if (bcdTime.Length >= 2)
                {
                    minute = ((bcdTime[1] >> 4) * 10) + (bcdTime[1] & 0x0F);
                }

                // 验证时间有效性
                if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60)
                {
                    return $"{hour:D2}:{minute:D2}";
                }
                else
                {
                    return $"无效时间({hour}:{minute})";
                }
            }
            catch (Exception ex)
            {
                return $"无效时间({ex.Message})";
            }
        }

        // 格式化BCD日期为字符串
        private string FormatBcdDateToString(byte[]? bcdDate)
        {
            if (bcdDate == null)
                return "无效日期(空)";

            try
            {
                // 如果长度不是3，尝试处理现有数据
                int year = 2000;
                int month = 1;
                int day = 1;

                if (bcdDate.Length >= 1)
                {
                    year = 2000 + ((bcdDate[0] >> 4) * 10) + (bcdDate[0] & 0x0F);
                }
                if (bcdDate.Length >= 2)
                {
                    month = ((bcdDate[1] >> 4) * 10) + (bcdDate[1] & 0x0F);
                }
                if (bcdDate.Length >= 3)
                {
                    day = ((bcdDate[2] >> 4) * 10) + (bcdDate[2] & 0x0F);
                }

                // 验证日期有效性
                if (month >= 1 && month <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month))
                {
                    return $"{year:D4}-{month:D2}-{day:D2}";
                }
                else
                {
                    return $"无效日期({year}-{month}-{day})";
                }
            }
            catch (Exception ex)
            {
                return $"无效日期({ex.Message})";
            }
        }

        // 格式化BCD时间为字符串
        private void UpdateStrategyList()
        {
            // 确保在UI线程上执行
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateStrategyList));
                return;
            }
            
            try
            {
                AddLog("开始更新策略列表");
                
                if (lstStrategies != null)
                {
                    // 保存当前选中的策略ID（而不是索引，更可靠）
                    uint selectedStrategyId = 0;
                    if (lstStrategies.SelectedIndex != -1 && strategyList != null && lstStrategies.SelectedIndex < strategyList.Count)
                    {
                        selectedStrategyId = strategyList[lstStrategies.SelectedIndex].StrategyId;
                    }

                    // 清空列表并重新添加项目
                    lstStrategies.Items?.Clear();
                    
                    int newSelectedIndex = -1;
                    if (strategyList != null && strategyList.Count > 0)
                    {
                        // 添加策略项到列表
                        for (int i = 0; i < strategyList.Count; i++)
                        {
                            var strategy = strategyList[i];
                            if (strategy != null)
                            {
                                string status = (strategy.EnabledFlag == 1) ? "启用" : "禁用";
                                lstStrategies.Items?.Add("策略 0x" + strategy.StrategyId.ToString("X8") + " - " + status);
                                
                                // 如果找到之前选中的策略，记录其新索引
                                if (strategy.StrategyId == selectedStrategyId)
                                {
                                    newSelectedIndex = i;
                                }
                            }
                        }

                        // 设置选中项
                        if (newSelectedIndex >= 0 && newSelectedIndex < lstStrategies.Items?.Count)
                        {
                            lstStrategies.SelectedIndex = newSelectedIndex;
                            AddLog($"恢复选中策略: 0x{selectedStrategyId:X8} 索引: {newSelectedIndex}");
                        }
                        else if (lstStrategies.Items?.Count > 0)
                        {
                            // 默认选中第一项
                            lstStrategies.SelectedIndex = 0;
                            AddLog("默认选中第一项策略");
                        }
                    }

                    // 强制触发选中事件以显示详情
                    if (lstStrategies.SelectedIndex != -1 && lstStrategies_SelectedIndexChanged != null)
                    {
                        AddLog($"主动触发选中事件，索引: {lstStrategies.SelectedIndex}");
                        lstStrategies_SelectedIndexChanged(lstStrategies, EventArgs.Empty);
                    }
                    else if (lstStrategies.Items?.Count == 0)
                    {
                        // 如果列表为空，清空详情显示
                        AddLog("策略列表为空，清空详情显示");
                        DisplayStrategyDetail(null);
                    }
                }
                else
                {
                    AddLog("错误: lstStrategies控件为空");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"更新策略列表时发生错误: {ex.Message}\n{ex.StackTrace}";
                AddLog(errorMsg);
            }
        }

        private void labelStrategyDetail_Click(object sender, EventArgs e)
        {

        }
    }

    // 条件数据模型基类
    public abstract class BaseCondition
    {
        public byte SIID { get; set; } // 2字节服务ID (这里简化为单字节，使用时可组合)
        public byte SIIDHigh { get; set; } // SIID高字节
        public byte CIID { get; set; } // 2字节特征ID (这里简化为单字节，使用时可组合)
        public byte CIIDHigh { get; set; } // CIID高字节
        public byte DataType { get; set; } // 1字节数据类型
        public uint DataValue { get; set; } // 4字节数据值

        // 获取数据类型显示文本
        public string GetDataTypeText()
        {
            switch (DataType)
            {
                case 0x00: return "bool";  
                case 0x01: return "int8";   
                case 0x02: return "uint8";  
                case 0x03: return "int16";  
                case 0x04: return "uint16"; 
                case 0x05: return "int32";  
                case 0x06: return "uint32"; 
                default: return "未知";  
            }
        }
    }

    // 触发条件数据模型
    public class TriggerCondition : BaseCondition
    {
        public byte[] MacAddress { get; set; } = new byte[6]; // 6字节Mac地址
        public byte LogicType { get; set; } // 1字节判断逻辑

        // 获取判断逻辑显示文本
        public string GetLogicTypeText()
        {
            switch (LogicType)
            {
                case 0x00: return "等于";      
                case 0x01: return "不等于";    
                case 0x02: return "大于";      
                case 0x03: return "大于等于";  
                case 0x04: return "小于";      
                case 0x05: return "小于等于";  
                default: return "未知";        
            }
        }

        // 克隆方法
        public TriggerCondition Clone()
        {
            return new TriggerCondition
            {
                MacAddress = (byte[])this.MacAddress.Clone(),
                SIID = this.SIID,
                SIIDHigh = this.SIIDHigh,
                CIID = this.CIID,
                CIIDHigh = this.CIIDHigh,
                DataType = this.DataType,
                DataValue = this.DataValue,
                LogicType = this.LogicType
            };
        }

        // 转换为显示字符串
        public override string ToString()
        {
            string macStr = MacAddress != null ? BitConverter.ToString(MacAddress).Replace("-", ":") : "[无效MAC]";
            ushort siid = (ushort)((SIIDHigh << 8) | SIID);
            ushort ciid = (ushort)((CIIDHigh << 8) | CIID);
            return $"MAC: {macStr}, SIID: 0x{siid:X4}, CIID: 0x{ciid:X4}, {GetDataTypeText()} {GetLogicTypeText()} 0x{DataValue:X8}";
        }
    }

    // 状态条件数据模型（与触发条件相同）
    public class StatusCondition : TriggerCondition
    {
        // 克隆方法（重写以返回StatusCondition类型）
        public new StatusCondition Clone()
        {
            return new StatusCondition
            {
                MacAddress = (byte[])this.MacAddress.Clone(),
                SIID = this.SIID,
                SIIDHigh = this.SIIDHigh,
                CIID = this.CIID,
                CIIDHigh = this.CIIDHigh,
                DataType = this.DataType,
                DataValue = this.DataValue,
                LogicType = this.LogicType
            };
        }
    }

    // 动作条件数据模型
    public class ActionCondition : BaseCondition
    {
        // 克隆方法
        public ActionCondition Clone()
        {
            return new ActionCondition
            {
                SIID = this.SIID,
                SIIDHigh = this.SIIDHigh,
                CIID = this.CIID,
                CIIDHigh = this.CIIDHigh,
                DataType = this.DataType,
                DataValue = this.DataValue
            };
        }

        // 转换为显示字符串
        public override string ToString()
        {
            ushort siid = (ushort)((SIIDHigh << 8) | SIID);
            ushort ciid = (ushort)((CIIDHigh << 8) | CIID);
            return $"SIID: 0x{siid:X4}, CIID: 0x{ciid:X4}, {GetDataTypeText()} = 0x{DataValue:X8}";
        }
    }

    // 策略数据类
    public class StrategyData
    {
        public uint StrategyId { get; set; } // 4字节策略ID
        public byte[] StartDate { get; set; } = new byte[3]; // 3字节开始日期(BCD)
        public byte[] EndDate { get; set; } = new byte[3]; // 3字节结束日期(BCD)
        public byte[] StartTime { get; set; } = new byte[2]; // 2字节开始时间(BCD)
        public byte[] EndTime { get; set; } = new byte[2]; // 2字节结束时间(BCD)
        public ushort WeekHolidayData { get; set; } // 2字节星期-节假日策略
        
        // 替换计数字段为实际条件列表
        public List<TriggerCondition> TriggerConditions { get; set; } = new List<TriggerCondition>();
        public List<StatusCondition> StatusConditions { get; set; } = new List<StatusCondition>();
        public List<ActionCondition> ActionConditions { get; set; } = new List<ActionCondition>();
        
        // 保留计数属性用于兼容性
        public byte TriggerCount { get { return (byte)TriggerConditions.Count; } set { /* 忽略设置，由列表大小决定 */ } }
        public byte StatusCount { get { return (byte)StatusConditions.Count; } set { /* 忽略设置，由列表大小决定 */ } }
        public byte ActionCount { get { return (byte)ActionConditions.Count; } set { /* 忽略设置，由列表大小决定 */ } }
        
        public byte EnabledFlag { get; set; } // 1字节策略使能位

        // 克隆方法
            public StrategyData Clone()
            {
                return new StrategyData
                {
                    StrategyId = this.StrategyId,
                    StartDate = (byte[])this.StartDate.Clone(),
                    EndDate = (byte[])this.EndDate.Clone(),
                    StartTime = (byte[])this.StartTime.Clone(),
                    EndTime = (byte[])this.EndTime.Clone(),
                    WeekHolidayData = this.WeekHolidayData,
                    EnabledFlag = this.EnabledFlag,
                    // 克隆条件列表
                    TriggerConditions = this.TriggerConditions.Select(t => t.Clone()).ToList(),
                    StatusConditions = this.StatusConditions.Select(s => s.Clone()).ToList(),
                    ActionConditions = this.ActionConditions.Select(a => a.Clone()).ToList()
                };
        }

        // 转换为显示字符串
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"策略ID: 0x{StrategyId:X8}");
            sb.AppendLine($"开始日期: {FormatBcdDate(StartDate)}");
            sb.AppendLine($"结束日期: {FormatBcdDate(EndDate)}");
            sb.AppendLine($"开始时间: {FormatBcdTime(StartTime)}");
            sb.AppendLine($"结束时间: {FormatBcdTime(EndTime)}");
            string weekHolidayText = FormatWeekHolidayData();
            sb.AppendLine("星期-节假日策略: 0x" + WeekHolidayData.ToString("X4") + " (" + weekHolidayText + ")");
            sb.AppendLine($"触发条件数: {TriggerCount}");
            sb.AppendLine($"状态条件数: {StatusCount}");
            sb.AppendLine($"动作条件数: {ActionCount}");
            string status = (EnabledFlag & 0x01) != 0 ? "启用" : "禁用";
            sb.AppendLine("使能状态: " + status);
            return sb.ToString();
        }

        // 格式化BCD日期
        private string FormatBcdDate(byte[] bcdDate)
        {
            if (bcdDate == null || bcdDate.Length != 3)
                return "无效日期";
            
            int year = 2000 + ((bcdDate[0] >> 4) * 10) + (bcdDate[0] & 0x0F);
            int month = ((bcdDate[1] >> 4) * 10) + (bcdDate[1] & 0x0F);
            int day = ((bcdDate[2] >> 4) * 10) + (bcdDate[2] & 0x0F);
            
            return $"20{year.ToString("D2")}-{month.ToString("D2")}-{day.ToString("D2")}";
        }

        // 格式化BCD时间
        private string FormatBcdTime(byte[] bcdTime)
        {
            if (bcdTime == null || bcdTime.Length != 2)
                return "无效时间";
            
            int hour = ((bcdTime[0] >> 4) * 10) + (bcdTime[0] & 0x0F);
            int minute = ((bcdTime[1] >> 4) * 10) + (bcdTime[1] & 0x0F);
            
            return hour.ToString("D2") + ":" + minute.ToString("D2");
        }

        // 格式化星期-节假日数据
        private string FormatWeekHolidayData()
        {
            StringBuilder sb = new StringBuilder();
            
            // 星期模式
            if ((WeekHolidayData & 0x80) != 0)
            {
                sb.Append("星期模式: ");
                if ((WeekHolidayData & 0x01) != 0) sb.Append("周日 ");
                if ((WeekHolidayData & 0x02) != 0) sb.Append("周一 ");
                if ((WeekHolidayData & 0x04) != 0) sb.Append("周二 ");
                if ((WeekHolidayData & 0x08) != 0) sb.Append("周三 ");
                if ((WeekHolidayData & 0x10) != 0) sb.Append("周四 ");
                if ((WeekHolidayData & 0x20) != 0) sb.Append("周五 ");
                if ((WeekHolidayData & 0x40) != 0) sb.Append("周六 ");
                sb.AppendLine();
            }
            
            // 节假日模式
            if ((WeekHolidayData & 0x100) != 0)
            {
                sb.Append("节假日模式: ");
                sb.Append((WeekHolidayData & 0x200) != 0 ? "法定节假日" : "工作日");
                sb.AppendLine();
            }
            
            // 逻辑关系
            sb.Append("逻辑关系: ");
            sb.Append((WeekHolidayData & 0x400) != 0 ? "与" : "或");
            
            return sb.ToString();
        }
    }

    // 条件编辑表单
    public partial class ConditionEditForm : Form
    {
        // 控件声明
        private Label? labelMacAddress; // 标记为可为null
        private TextBox? txtMacAddress; // 标记为可为null
        private Label? labelSIID; // 标记为可为null
        private TextBox? txtSIID; // 标记为可为null
        private Label? labelCIID; // 标记为可为null
        private TextBox? txtCIID; // 标记为可为null
        private Label? labelDataType; // 标记为可为null
        private ComboBox? cmbDataType; // 标记为可为null
        private Label? labelDataValue; // 标记为可为null
        private TextBox? txtDataValue; // 标记为可为null
        private Label? labelLogicType; // 标记为可为null
        private ComboBox? cmbLogicType; // 标记为可为null
        private Button? btnOK; // 标记为可为null
        private Button? btnCancel; // 标记为可为null

        // 属性
        public bool IsActionCondition { get; private set; }
        public TriggerCondition? TriggerCondition { get; private set; } // 标记为可为null
        public StatusCondition? StatusCondition { get; private set; } // 标记为可为null
        public ActionCondition? ActionCondition { get; private set; } // 标记为可为null

        // 触发条件和状态条件构造函数（添加布尔参数以消除二义性）
        public ConditionEditForm(TriggerCondition? triggerCondition = null, StatusCondition? statusCondition = null, bool isTriggerOrStatusCondition = true)
        {
            InitializeComponent();
            IsActionCondition = false;

            // 初始化数据类型下拉框
            if (cmbDataType != null && cmbDataType.Items != null)
            {
                cmbDataType.Items.AddRange(new object[]
                {
                    new { Text = "0x00 - bool", Value = 0x00 },
                    new { Text = "0x01 - int8", Value = 0x01 },
                    new { Text = "0x02 - uint8", Value = 0x02 },
                    new { Text = "0x03 - int16", Value = 0x03 },
                    new { Text = "0x04 - uint16", Value = 0x04 },
                    new { Text = "0x05 - int32", Value = 0x05 },
                    new { Text = "0x06 - uint32", Value = 0x06 }
                });
            }
            if (cmbDataType != null)
            {
                cmbDataType.DisplayMember = "Text";
                cmbDataType.ValueMember = "Value";
            }
            if (cmbDataType != null) cmbDataType.SelectedIndex = 0;

            // 初始化逻辑类型下拉框
            if (cmbLogicType != null && cmbLogicType.Items != null)
            {
                cmbLogicType.Items.AddRange(new object[]
                {
                    new { Text = "0x00 - 等于", Value = 0x00 },
                    new { Text = "0x01 - 不等于", Value = 0x01 },
                    new { Text = "0x02 - 大于", Value = 0x02 },
                    new { Text = "0x03 - 大于等于", Value = 0x03 },
                    new { Text = "0x04 - 小于", Value = 0x04 },
                    new { Text = "0x05 - 小于等于", Value = 0x05 }
                });
            }
            if (cmbLogicType != null)
            {
                cmbLogicType.DisplayMember = "Text";
                cmbLogicType.ValueMember = "Value";
            }
            if (cmbLogicType != null) cmbLogicType.SelectedIndex = 0;

            if (statusCondition != null)
            {
                // 编辑状态条件
                StatusCondition = statusCondition.Clone();
                Text = "编辑状态条件";
                LoadConditionData();
            }
            else if (triggerCondition != null)
            {
                // 编辑触发条件
                TriggerCondition = triggerCondition.Clone();
                Text = "编辑触发条件";
                LoadConditionData();
            }
            else
            {
                // 新建模式
                TriggerCondition = new TriggerCondition();
                Text = "新建触发条件";
            }
        }

        // 动作条件构造函数（添加布尔参数以消除二义性）
        public ConditionEditForm(ActionCondition? actionCondition = null, bool isActionCondition = true)
        {
            InitializeComponent();
            IsActionCondition = true;
            
            // 隐藏MAC地址和逻辑类型控件
            labelMacAddress?.Hide();
            txtMacAddress?.Hide();
            labelLogicType?.Hide();
            cmbLogicType?.Hide();

            // 初始化数据类型下拉框
            if (cmbDataType != null && cmbDataType.Items != null)
            {
                cmbDataType.Items.AddRange(new object[]
                {
                    new { Text = "0x00 - bool", Value = 0x00 },
                    new { Text = "0x01 - int8", Value = 0x01 },
                    new { Text = "0x02 - uint8", Value = 0x02 },
                    new { Text = "0x03 - int16", Value = 0x03 },
                    new { Text = "0x04 - uint16", Value = 0x04 },
                    new { Text = "0x05 - int32", Value = 0x05 },
                    new { Text = "0x06 - uint32", Value = 0x06 }
                });
            }
            if (cmbDataType != null)
            {
                cmbDataType.DisplayMember = "Text";
                cmbDataType.ValueMember = "Value";
            }
            if (cmbDataType != null) cmbDataType.SelectedIndex = 0;

            if (actionCondition != null)
            {
                // 编辑模式
                ActionCondition = actionCondition.Clone();
                Text = "编辑动作条件";
                LoadConditionData();
            }
            else
            {
                // 新建模式
                ActionCondition = new ActionCondition();
                Text = "新建动作条件";
            }
        }

        private void InitializeComponent()
        {
            this.labelMacAddress = new System.Windows.Forms.Label();
            this.txtMacAddress = new System.Windows.Forms.TextBox();
            this.labelSIID = new System.Windows.Forms.Label();
            this.txtSIID = new System.Windows.Forms.TextBox();
            this.labelCIID = new System.Windows.Forms.Label();
            this.txtCIID = new System.Windows.Forms.TextBox();
            this.labelDataType = new System.Windows.Forms.Label();
            this.cmbDataType = new System.Windows.Forms.ComboBox();
            this.labelDataValue = new System.Windows.Forms.Label();
            this.txtDataValue = new System.Windows.Forms.TextBox();
            this.labelLogicType = new System.Windows.Forms.Label();
            this.cmbLogicType = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelMacAddress
            // 
            this.labelMacAddress.AutoSize = true;
            this.labelMacAddress.Location = new System.Drawing.Point(12, 20);
            this.labelMacAddress.Name = "labelMacAddress";
            this.labelMacAddress.Size = new System.Drawing.Size(76, 17);
            this.labelMacAddress.TabIndex = 0;
            this.labelMacAddress.Text = "Mac地址:";
            // 
            // txtMacAddress
            // 
            this.txtMacAddress.Location = new System.Drawing.Point(94, 17);
            this.txtMacAddress.Name = "txtMacAddress";
            this.txtMacAddress.Size = new System.Drawing.Size(200, 23);
            this.txtMacAddress.TabIndex = 1;
            this.txtMacAddress.Text = "00:00:00:00:00:00";
            // 
            // labelSIID
            // 
            this.labelSIID.AutoSize = true;
            this.labelSIID.Location = new System.Drawing.Point(12, 60);
            this.labelSIID.Name = "labelSIID";
            this.labelSIID.Size = new System.Drawing.Size(44, 17);
            this.labelSIID.TabIndex = 2;
            this.labelSIID.Text = "SIID:";
            // 
            // txtSIID
            // 
            this.txtSIID.Location = new System.Drawing.Point(94, 57);
            this.txtSIID.Name = "txtSIID";
            this.txtSIID.Size = new System.Drawing.Size(100, 23);
            this.txtSIID.TabIndex = 3;
            this.txtSIID.Text = "0x0000";
            // 
            // labelCIID
            // 
            this.labelCIID.AutoSize = true;
            this.labelCIID.Location = new System.Drawing.Point(210, 60);
            this.labelCIID.Name = "labelCIID";
            this.labelCIID.Size = new System.Drawing.Size(44, 17);
            this.labelCIID.TabIndex = 4;
            this.labelCIID.Text = "CIID:";
            // 
            // txtCIID
            // 
            this.txtCIID.Location = new System.Drawing.Point(294, 57);
            this.txtCIID.Name = "txtCIID";
            this.txtCIID.Size = new System.Drawing.Size(100, 23);
            this.txtCIID.TabIndex = 5;
            this.txtCIID.Text = "0x0000";
            // 
            // labelDataType
            // 
            this.labelDataType.AutoSize = true;
            this.labelDataType.Location = new System.Drawing.Point(12, 100);
            this.labelDataType.Name = "labelDataType";
            this.labelDataType.Size = new System.Drawing.Size(76, 17);
            this.labelDataType.TabIndex = 6;
            this.labelDataType.Text = "数据类型:";
            // 
            // cmbDataType
            // 
            this.cmbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataType.Location = new System.Drawing.Point(94, 97);
            this.cmbDataType.Name = "cmbDataType";
            this.cmbDataType.Size = new System.Drawing.Size(150, 25);
            this.cmbDataType.TabIndex = 7;
            // 
            // labelDataValue
            // 
            this.labelDataValue.AutoSize = true;
            this.labelDataValue.Location = new System.Drawing.Point(260, 100);
            this.labelDataValue.Name = "labelDataValue";
            this.labelDataValue.Size = new System.Drawing.Size(76, 17);
            this.labelDataValue.TabIndex = 8;
            this.labelDataValue.Text = "数据值:";
            // 
            // txtDataValue
            // 
            this.txtDataValue.Location = new System.Drawing.Point(342, 97);
            this.txtDataValue.Name = "txtDataValue";
            this.txtDataValue.Size = new System.Drawing.Size(100, 23);
            this.txtDataValue.TabIndex = 9;
            this.txtDataValue.Text = "0x00000000";
            // 
            // labelLogicType
            // 
            this.labelLogicType.AutoSize = true;
            this.labelLogicType.Location = new System.Drawing.Point(12, 140);
            this.labelLogicType.Name = "labelLogicType";
            this.labelLogicType.Size = new System.Drawing.Size(76, 17);
            this.labelLogicType.TabIndex = 10;
            this.labelLogicType.Text = "判断逻辑:";
            // 
            // cmbLogicType
            // 
            this.cmbLogicType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogicType.Location = new System.Drawing.Point(94, 137);
            this.cmbLogicType.Name = "cmbLogicType";
            this.cmbLogicType.Size = new System.Drawing.Size(150, 25);
            this.cmbLogicType.TabIndex = 11;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(246, 180);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 12;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(352, 180);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ConditionEditForm
            // 
            this.ClientSize = new System.Drawing.Size(464, 222);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cmbLogicType);
            this.Controls.Add(this.labelLogicType);
            this.Controls.Add(this.txtDataValue);
            this.Controls.Add(this.labelDataValue);
            this.Controls.Add(this.cmbDataType);
            this.Controls.Add(this.labelDataType);
            this.Controls.Add(this.txtCIID);
            this.Controls.Add(this.labelCIID);
            this.Controls.Add(this.txtSIID);
            this.Controls.Add(this.labelSIID);
            this.Controls.Add(this.txtMacAddress);
            this.Controls.Add(this.labelMacAddress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConditionEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "条件编辑";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadConditionData()
        {
            if (IsActionCondition && ActionCondition != null)
            {
                // 加载动作条件数据
                if (txtSIID != null)
            {
                txtSIID.Text = $"0x{((ActionCondition.SIIDHigh << 8) | ActionCondition.SIID):X4}";
            }
            if (txtCIID != null)
            {
                txtCIID.Text = $"0x{((ActionCondition.CIIDHigh << 8) | ActionCondition.CIID):X4}";
            }
            if (cmbDataType != null)
            {
                cmbDataType.SelectedValue = ActionCondition.DataType;
            }
            if (txtDataValue != null)
            {
                txtDataValue.Text = $"0x{ActionCondition.DataValue:X8}";
            }
            }
            else if (StatusCondition != null)
            {
                // 加载状态条件数据
                if (txtMacAddress != null)
            {
                txtMacAddress.Text = BitConverter.ToString(StatusCondition.MacAddress).Replace("-", ":");
            }
            if (StatusCondition != null) {
                if (txtSIID != null)
                {
                    txtSIID.Text = $"0x{((StatusCondition.SIIDHigh << 8) | StatusCondition.SIID):X4}";
                }
                if (txtCIID != null)
                {
                    txtCIID.Text = $"0x{((StatusCondition.CIIDHigh << 8) | StatusCondition.CIID):X4}";
                }
                if (cmbDataType != null)
                {
                    cmbDataType.SelectedValue = StatusCondition.DataType;
                }
                if (txtDataValue != null)
                {
                    txtDataValue.Text = $"0x{StatusCondition.DataValue:X8}";
                }
                if (cmbLogicType != null)
                {
                    cmbLogicType.SelectedValue = StatusCondition.LogicType;
                }
            }
            }
            else if (TriggerCondition != null)
            {
                // 加载触发条件数据
                if (txtMacAddress != null)
                {
                    txtMacAddress.Text = BitConverter.ToString(TriggerCondition.MacAddress).Replace("-", ":");
                }
                if (txtSIID != null)
                {
                    txtSIID.Text = $"0x{((TriggerCondition.SIIDHigh << 8) | TriggerCondition.SIID):X4}";
                }
                if (txtCIID != null)
                {
                    txtCIID.Text = $"0x{((TriggerCondition.CIIDHigh << 8) | TriggerCondition.CIID):X4}";
                }
                if (cmbDataType != null)
                {
                    cmbDataType.SelectedValue = TriggerCondition.DataType;
                }
                if (txtDataValue != null)
                {
                    txtDataValue.Text = $"0x{TriggerCondition.DataValue:X8}";
                }
                if (cmbLogicType != null)
                {
                    cmbLogicType.SelectedValue = TriggerCondition.LogicType;
                }
            }
        }

        private bool ValidateForm()
        {   
            // 验证SIID
            string siidText = (txtSIID?.Text ?? "").Trim().TrimStart('0', 'x', 'X');
            // 如果移除前导字符后为空，则默认为0
            if (string.IsNullOrEmpty(siidText)) siidText = "0";
            if (!uint.TryParse(siidText, System.Globalization.NumberStyles.HexNumber, null, out uint siid))
            {
                MessageBox.Show("请输入有效的SIID", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtSIID?.Focus();
                return false;
            }

            // 验证CIID
            string ciidText = (txtCIID?.Text ?? "").Trim().TrimStart('0', 'x', 'X');
            // 如果移除前导字符后为空，则默认为0
            if (string.IsNullOrEmpty(ciidText)) ciidText = "0";
            if (!uint.TryParse(ciidText, System.Globalization.NumberStyles.HexNumber, null, out uint ciid))
            {
                MessageBox.Show("请输入有效的CIID", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCIID?.Focus();
                return false;
            }

            // 验证数据值
            string dataValueText = (txtDataValue?.Text ?? "").Trim().TrimStart('0', 'x', 'X');
            // 如果移除前导字符后为空，则默认为0
            if (string.IsNullOrEmpty(dataValueText)) dataValueText = "0";
            if (!uint.TryParse(dataValueText, System.Globalization.NumberStyles.HexNumber, null, out uint dataValue))
            {
                MessageBox.Show("请输入有效的数据值", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDataValue?.Focus();
                return false;
            }

            // 对于非动作条件，验证MAC地址
            if (!IsActionCondition && txtMacAddress != null)
            {
                string[] macParts = txtMacAddress.Text.Split(':');
                if (macParts.Length != 6)
                {
                    MessageBox.Show("MAC地址格式不正确，应为XX:XX:XX:XX:XX:XX", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtMacAddress.Focus();
                    return false;
                }
            }

            return true;
        }

        private void btnOK_Click(object? sender, EventArgs e)
        {   
            if (!ValidateForm())
                return;

            // 使用TryParse避免格式错误导致的异常
            string siidText = txtSIID?.Text?.Trim()?.TrimStart('0', 'x', 'X') ?? string.Empty;
            string ciidText = txtCIID?.Text?.Trim()?.TrimStart('0', 'x', 'X') ?? string.Empty;
            string dataValueText = txtDataValue?.Text?.Trim()?.TrimStart('0', 'x', 'X') ?? string.Empty;
            
            uint siid = uint.TryParse(siidText, System.Globalization.NumberStyles.HexNumber, null, out uint siidValue) ? siidValue : 0;
            uint ciid = uint.TryParse(ciidText, System.Globalization.NumberStyles.HexNumber, null, out uint ciidValue) ? ciidValue : 0;
            uint dataValue = uint.TryParse(dataValueText, System.Globalization.NumberStyles.HexNumber, null, out uint dataValueValue) ? dataValueValue : 0;
            byte dataType = cmbDataType?.SelectedItem != null ? (byte)((dynamic)cmbDataType.SelectedItem).Value : (byte)0; // 添加null检查

            if (IsActionCondition && ActionCondition != null)
            {
                // 保存动作条件
                ActionCondition.SIIDHigh = (byte)((siid >> 8) & 0xFF);
                ActionCondition.SIID = (byte)(siid & 0xFF);
                ActionCondition.CIIDHigh = (byte)((ciid >> 8) & 0xFF);
                ActionCondition.CIID = (byte)(ciid & 0xFF);
                ActionCondition.DataType = dataType;
                ActionCondition.DataValue = dataValue;
            }
            else if (StatusCondition != null)
            {
                // 保存状态条件
                if (txtMacAddress != null)
                {
                    string[] macParts = txtMacAddress.Text.Split(':');
                    // 确保macParts数组长度足够
                    for (int i = 0; i < 6 && i < macParts.Length; i++)
                    {
                        StatusCondition.MacAddress[i] = byte.TryParse(macParts[i], System.Globalization.NumberStyles.HexNumber, null, out byte value) ? value : (byte)0;
                    }
                }
                StatusCondition.SIIDHigh = (byte)((siid >> 8) & 0xFF);
                StatusCondition.SIID = (byte)(siid & 0xFF);
                StatusCondition.CIIDHigh = (byte)((ciid >> 8) & 0xFF);
                StatusCondition.CIID = (byte)(ciid & 0xFF);
                StatusCondition.DataType = dataType;
                StatusCondition.DataValue = dataValue;
                StatusCondition.LogicType = cmbLogicType?.SelectedItem != null ? (byte)((dynamic)cmbLogicType.SelectedItem).Value : (byte)0;
            }
            else if (TriggerCondition != null)
            {
                // 保存触发条件
                if (txtMacAddress != null)
                {
                    string[] macParts = txtMacAddress.Text.Split(':');
                    // 确保macParts数组长度足够
                    for (int i = 0; i < 6 && i < macParts.Length; i++)
                    {
                        TriggerCondition.MacAddress[i] = byte.TryParse(macParts[i], System.Globalization.NumberStyles.HexNumber, null, out byte value) ? value : (byte)0;
                    }
                }
                TriggerCondition.SIIDHigh = (byte)((siid >> 8) & 0xFF);
                TriggerCondition.SIID = (byte)(siid & 0xFF);
                TriggerCondition.CIIDHigh = (byte)((ciid >> 8) & 0xFF);
                TriggerCondition.CIID = (byte)(ciid & 0xFF);
                TriggerCondition.DataType = dataType;
                TriggerCondition.DataValue = dataValue;
                TriggerCondition.LogicType = cmbLogicType?.SelectedItem != null ? (byte)((dynamic)cmbLogicType.SelectedItem).Value : (byte)0;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {   
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    // 策略配置表单
    public partial class StrategyConfigForm : Form
    {
        // 表单控件声明
        private Label? label1; // 标记为可为null
        private TextBox? txtStrategyId; // 标记为可为null
        private Button? btnGenerateId; // 标记为可为null
        private Label? label2; // 标记为可为null
        private DateTimePicker? dtpStartDate; // 标记为可为null
        private Label? label3; // 标记为可为null
        private DateTimePicker? dtpEndDate; // 标记为可为null
        private Label? label4; // 标记为可为null
        private DateTimePicker? dtpStartTime; // 标记为可为null
        private Label? label5; // 标记为可为null
        private DateTimePicker? dtpEndTime; // 标记为可为null
        private Label? label6; // 标记为可为null
        private CheckBox? chkSunday; // 标记为可为null
        private CheckBox? chkMonday; // 标记为可为null
        private CheckBox? chkTuesday; // 标记为可为null
        private CheckBox? chkWednesday; // 标记为可为null
        private CheckBox? chkThursday; // 标记为可为null
        private CheckBox? chkFriday; // 标记为可为null
        private CheckBox? chkSaturday; // 标记为可为null
        private CheckBox? chkWeekMode; // 标记为可为null
        private CheckBox? chkHolidayMode; // 标记为可为null
        private CheckBox? chkHolidayWorkday; // 标记为可为null
        private CheckBox? chkLogicAnd; // 标记为可为null
        private CheckBox? chkLogicOr; // 标记为可为null
        private CheckBox? chkEnabled; // 标记为可为null
        private Button? btnOK; // 标记为可为null
        private Button? btnCancel; // 标记为可为null
        private GroupBox? grpWeekConfig; // 标记为可为null
        private GroupBox? grpDateConfig; // 标记为可为null
        private GroupBox? grpTimeConfig; // 标记为可为null
        private GroupBox? grpLogicConfig; // 标记为可为null
        private GroupBox? grpOtherConfig; // 标记为可为null
        
        // 新增控件：条件列表和管理按钮
        private Label? labelTriggerConditions; // 标记为可为null
        private ListBox? lstTriggerConditions; // 标记为可为null
        private Button? btnAddTriggerCondition; // 标记为可为null
        private Button? btnEditTriggerCondition; // 标记为可为null
        private Button? btnDeleteTriggerCondition; // 标记为可为null
        private Label? labelStatusConditions; // 标记为可为null
        private ListBox? lstStatusConditions; // 标记为可为null
        private Button? btnAddStatusCondition; // 标记为可为null
        private Button? btnEditStatusCondition; // 标记为可为null
        private Button? btnDeleteStatusCondition; // 标记为可为null
        private Label? labelActionConditions; // 标记为可为null
        private ListBox? lstActionConditions; // 标记为可为null
        private Button? btnAddActionCondition; // 标记为可为null
        private Button? btnEditActionCondition; // 标记为可为null
        private Button? btnDeleteActionCondition; // 标记为可为null

        // 策略数据模型
        public StrategyData? StrategyData { get; private set; }

        // 构造函数
        public StrategyConfigForm(StrategyData? existingStrategy = null)
        {
            InitializeComponent();
            InitializeUI();
            
            // 如果是编辑模式，加载现有策略数据
            if (existingStrategy != null)
            {
                StrategyData = existingStrategy.Clone();
                LoadStrategyData();
                Text = "编辑策略";
            }
            else
            {
                // 新建模式，生成随机策略ID
                StrategyData = new StrategyData
                {
                    StrategyId = GenerateRandomStrategyId()
                };
                // 设置默认日期为今天，使用null条件操作符避免空引用
                if (dtpStartDate != null)
                {
                    dtpStartDate.Value = DateTime.Now;
                }
                if (dtpEndDate != null)
                {
                    dtpEndDate.Value = DateTime.Now.AddDays(30);
                }
                // 设置默认时间为当前时间，使用null条件操作符避免空引用
                if (dtpStartTime != null)
                {
                    dtpStartTime.Value = DateTime.Now;
                }
                if (dtpEndTime != null)
                {
                    dtpEndTime.Value = DateTime.Now.AddHours(1);
                }
            }
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtStrategyId = new System.Windows.Forms.TextBox();
            this.btnGenerateId = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.chkSunday = new System.Windows.Forms.CheckBox();
            this.chkMonday = new System.Windows.Forms.CheckBox();
            this.chkTuesday = new System.Windows.Forms.CheckBox();
            this.chkWednesday = new System.Windows.Forms.CheckBox();
            this.chkThursday = new System.Windows.Forms.CheckBox();
            this.chkFriday = new System.Windows.Forms.CheckBox();
            this.chkSaturday = new System.Windows.Forms.CheckBox();
            this.chkWeekMode = new System.Windows.Forms.CheckBox();
            this.chkHolidayMode = new System.Windows.Forms.CheckBox();
            this.chkHolidayWorkday = new System.Windows.Forms.CheckBox();
            this.chkLogicAnd = new System.Windows.Forms.CheckBox();
            this.chkLogicOr = new System.Windows.Forms.CheckBox();

            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpWeekConfig = new System.Windows.Forms.GroupBox();
            this.grpDateConfig = new System.Windows.Forms.GroupBox();
            this.grpTimeConfig = new System.Windows.Forms.GroupBox();
            this.grpLogicConfig = new System.Windows.Forms.GroupBox();
            this.grpOtherConfig = new System.Windows.Forms.GroupBox();
            this.grpDateConfig.SuspendLayout();
            this.grpTimeConfig.SuspendLayout();
            this.grpWeekConfig.SuspendLayout();
            this.grpLogicConfig.SuspendLayout();
            this.grpOtherConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "策略ID (hex):";
            this.label1.BringToFront();
            // 
            // txtStrategyId
            // 
            this.txtStrategyId.Location = new System.Drawing.Point(120, 12);
            this.txtStrategyId.Name = "txtStrategyId";
            this.txtStrategyId.Size = new System.Drawing.Size(120, 23);
            this.txtStrategyId.TabIndex = 1;
            this.txtStrategyId.BringToFront();
            // 
            // btnGenerateId
            // 
            this.btnGenerateId.Location = new System.Drawing.Point(246, 12);
            this.btnGenerateId.Name = "btnGenerateId";
            this.btnGenerateId.Size = new System.Drawing.Size(120, 23);
            this.btnGenerateId.TabIndex = 2;
            this.btnGenerateId.Text = "生成随机策略ID";
            this.btnGenerateId.UseVisualStyleBackColor = true;
            this.btnGenerateId.Click += new System.EventHandler(this.btnGenerateId_Click);
            this.btnGenerateId.BringToFront();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "开始日期:";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Location = new System.Drawing.Point(80, 22);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(150, 23);
            this.dtpStartDate.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(240, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "结束日期:";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Location = new System.Drawing.Point(314, 22);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(150, 23);
            this.dtpEndDate.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "开始时间:";
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpStartTime.Location = new System.Drawing.Point(80, 22);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.ShowUpDown = true;
            this.dtpStartTime.Size = new System.Drawing.Size(100, 23);
            this.dtpStartTime.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(240, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "结束时间:";
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpEndTime.Location = new System.Drawing.Point(314, 33);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.ShowUpDown = true;
            this.dtpEndTime.Size = new System.Drawing.Size(100, 23);
            this.dtpEndTime.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 40);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 17);
            this.label6.TabIndex = 10;
            this.label6.Text = "选择星期:";
            // 
            // chkSunday
            // 
            this.chkSunday.AutoSize = true;
            this.chkSunday.Location = new System.Drawing.Point(80, 40);
            this.chkSunday.Name = "chkSunday";
            this.chkSunday.Size = new System.Drawing.Size(44, 21);
            this.chkSunday.TabIndex = 11;
            this.chkSunday.Text = "周日";
            this.chkSunday.UseVisualStyleBackColor = true;
            // 
            // chkMonday
            // 
            this.chkMonday.AutoSize = true;
            this.chkMonday.Location = new System.Drawing.Point(140, 40);
            this.chkMonday.Name = "chkMonday";
            this.chkMonday.Size = new System.Drawing.Size(44, 21);
            this.chkMonday.TabIndex = 12;
            this.chkMonday.Text = "周一";
            this.chkMonday.UseVisualStyleBackColor = true;
            // 
            // chkTuesday
            // 
            this.chkTuesday.AutoSize = true;
            this.chkTuesday.Location = new System.Drawing.Point(200, 40);
            this.chkTuesday.Name = "chkTuesday";
            this.chkTuesday.Size = new System.Drawing.Size(44, 21);
            this.chkTuesday.TabIndex = 13;
            this.chkTuesday.Text = "周二";
            this.chkTuesday.UseVisualStyleBackColor = true;
            // 
            // chkWednesday
            // 
            this.chkWednesday.AutoSize = true;
            this.chkWednesday.Location = new System.Drawing.Point(260, 40);
            this.chkWednesday.Name = "chkWednesday";
            this.chkWednesday.Size = new System.Drawing.Size(44, 21);
            this.chkWednesday.TabIndex = 14;
            this.chkWednesday.Text = "周三";
            this.chkWednesday.UseVisualStyleBackColor = true;
            // 
            // chkThursday
            // 
            this.chkThursday.AutoSize = true;
            this.chkThursday.Location = new System.Drawing.Point(320, 40);
            this.chkThursday.Name = "chkThursday";
            this.chkThursday.Size = new System.Drawing.Size(44, 21);
            this.chkThursday.TabIndex = 15;
            this.chkThursday.Text = "周四";
            this.chkThursday.UseVisualStyleBackColor = true;
            // 
            // chkFriday
            // 
            this.chkFriday.AutoSize = true;
            this.chkFriday.Location = new System.Drawing.Point(380, 40);
            this.chkFriday.Name = "chkFriday";
            this.chkFriday.Size = new System.Drawing.Size(44, 21);
            this.chkFriday.TabIndex = 16;
            this.chkFriday.Text = "周五";
            this.chkFriday.UseVisualStyleBackColor = true;
            // 
            // chkSaturday
            // 
            this.chkSaturday.AutoSize = true;
            this.chkSaturday.Location = new System.Drawing.Point(440, 40);
            this.chkSaturday.Name = "chkSaturday";
            this.chkSaturday.Size = new System.Drawing.Size(44, 21);
            this.chkSaturday.TabIndex = 17;
            this.chkSaturday.Text = "周六";
            this.chkSaturday.UseVisualStyleBackColor = true;
            // 
            // chkWeekMode
            // 
            this.chkWeekMode.AutoSize = true;
            this.chkWeekMode.Location = new System.Drawing.Point(10, 15);
            this.chkWeekMode.Name = "chkWeekMode";
            this.chkWeekMode.Size = new System.Drawing.Size(89, 35);
            this.chkWeekMode.TabIndex = 18;
            this.chkWeekMode.Text = "启用星期模式";
            this.chkWeekMode.UseVisualStyleBackColor = true;
            this.chkWeekMode.CheckedChanged += new System.EventHandler(this.chkWeekMode_CheckedChanged);
            // 
            // chkHolidayMode
            // 
            this.chkHolidayMode.AutoSize = true;
            this.chkHolidayMode.Location = new System.Drawing.Point(10, 60);
            this.chkHolidayMode.Name = "chkHolidayMode";
            this.chkHolidayMode.Size = new System.Drawing.Size(101, 21);
            this.chkHolidayMode.TabIndex = 19;
            this.chkHolidayMode.Text = "启用节假日模式";
            this.chkHolidayMode.UseVisualStyleBackColor = true;
            this.chkHolidayMode.CheckedChanged += new System.EventHandler(this.chkHolidayMode_CheckedChanged);
            // 
            // chkHolidayWorkday
            // 
            this.chkHolidayWorkday.AutoSize = true;
            this.chkHolidayWorkday.Location = new System.Drawing.Point(140, 60);
            // 移除可能重叠的节假日工作控件，因为空间有限
            this.chkHolidayWorkday.Name = "chkHolidayWorkday";
            this.chkHolidayWorkday.Size = new System.Drawing.Size(113, 21);
            this.chkHolidayWorkday.TabIndex = 20;
            this.chkHolidayWorkday.Text = "法定节假日生效";
            this.chkHolidayWorkday.UseVisualStyleBackColor = true;
            // 
            // chkLogicAnd
            // 
            this.chkLogicAnd.AutoSize = true;
            this.chkLogicAnd.Location = new System.Drawing.Point(10, 80);
            this.chkLogicAnd.Name = "chkLogicAnd";
            this.chkLogicAnd.Size = new System.Drawing.Size(69, 21);
            this.chkLogicAnd.TabIndex = 21;
            this.chkLogicAnd.Text = "条件且";
            this.chkLogicAnd.UseVisualStyleBackColor = true;
            this.chkLogicAnd.CheckedChanged += new System.EventHandler(this.chkLogicAnd_CheckedChanged);
            // 
            // chkLogicOr
            // 
            this.chkLogicOr.AutoSize = true;
            this.chkLogicOr.Checked = true;
            this.chkLogicOr.Location = new System.Drawing.Point(80, 80);
            this.chkLogicOr.Name = "chkLogicOr";
            this.chkLogicOr.Size = new System.Drawing.Size(69, 21);
            this.chkLogicOr.TabIndex = 22;
            this.chkLogicOr.Text = "条件或";
            this.chkLogicOr.UseVisualStyleBackColor = true;
            this.chkLogicOr.CheckedChanged += new System.EventHandler(this.chkLogicOr_CheckedChanged);
            // 

            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(432, 15);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(69, 21);
            this.chkEnabled.TabIndex = 29;
            this.chkEnabled.Text = "启用策略";
            this.chkEnabled.UseVisualStyleBackColor = true;
            this.chkEnabled.BringToFront();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(330, 595); // 再次下移确定按钮
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 30;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(440, 595); // 再次下移取消按钮
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 31;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // grpWeekConfig
            // 
            this.grpWeekConfig.Controls.Add(this.label6);
            this.grpWeekConfig.Controls.Add(this.chkSunday);
            this.grpWeekConfig.Controls.Add(this.chkMonday);
            this.grpWeekConfig.Controls.Add(this.chkHolidayWorkday);
            this.grpWeekConfig.Controls.Add(this.chkTuesday);
            this.grpWeekConfig.Controls.Add(this.chkHolidayMode);
            this.grpWeekConfig.Controls.Add(this.chkWednesday);
            this.grpWeekConfig.Controls.Add(this.chkWeekMode);
            this.grpWeekConfig.Controls.Add(this.chkThursday);
            this.grpWeekConfig.Controls.Add(this.chkFriday);
            this.grpWeekConfig.Controls.Add(this.chkSaturday);
            this.grpWeekConfig.Controls.Add(this.chkLogicAnd);
            this.grpWeekConfig.Controls.Add(this.chkLogicOr);
            this.grpWeekConfig.Location = new System.Drawing.Point(12, 170);
            this.grpWeekConfig.Name = "grpWeekConfig";
            this.grpWeekConfig.Size = new System.Drawing.Size(530, 110);
            this.grpWeekConfig.TabIndex = 32;
            this.grpWeekConfig.TabStop = false;
            this.grpWeekConfig.Text = "星期和节假日配置及逻辑关系";
            // 
            // grpDateConfig
            // 
            this.grpDateConfig.Controls.Add(this.label2);
            this.grpDateConfig.Controls.Add(this.dtpStartDate);
            this.grpDateConfig.Controls.Add(this.label3);
            this.grpDateConfig.Controls.Add(this.dtpEndDate);
            this.grpDateConfig.Location = new System.Drawing.Point(12, 50);
            this.grpDateConfig.Name = "grpDateConfig";
            this.grpDateConfig.Size = new System.Drawing.Size(530, 60);
            this.grpDateConfig.TabIndex = 33;
            this.grpDateConfig.TabStop = false;
            this.grpDateConfig.Text = "日期配置";
            // 
            // grpTimeConfig
            // 
            this.grpTimeConfig.Controls.Add(this.label4);
            this.grpTimeConfig.Controls.Add(this.dtpStartTime);
            this.grpTimeConfig.Controls.Add(this.label5);
            this.grpTimeConfig.Controls.Add(this.dtpEndTime);
            this.grpTimeConfig.Location = new System.Drawing.Point(12, 110);
            this.grpTimeConfig.Name = "grpTimeConfig";
            this.grpTimeConfig.Size = new System.Drawing.Size(530, 60);
            this.grpTimeConfig.TabIndex = 34;
            this.grpTimeConfig.TabStop = false;
            this.grpTimeConfig.Text = "时间配置";
            // 
            // grpLogicConfig (已整合到grpWeekConfig中)
            // 
            // this.grpLogicConfig.Controls.Add(this.chkLogicAnd);
            // this.grpLogicConfig.Controls.Add(this.chkLogicOr);
            // this.grpLogicConfig.Location = new System.Drawing.Point(12, 220);
            // this.grpLogicConfig.Name = "grpLogicConfig";
            // this.grpLogicConfig.Size = new System.Drawing.Size(120, 60);
            // this.grpLogicConfig.TabIndex = 35;
            // this.grpLogicConfig.TabStop = false;
            // this.grpLogicConfig.Text = "逻辑关系";
            // 
            // labelTriggerConditions
            // 
            this.labelTriggerConditions = new System.Windows.Forms.Label();
            this.labelTriggerConditions.AutoSize = true;
            this.labelTriggerConditions.Location = new System.Drawing.Point(6, 25);
            this.labelTriggerConditions.Name = "labelTriggerConditions";
            this.labelTriggerConditions.Size = new System.Drawing.Size(76, 17);
            this.labelTriggerConditions.TabIndex = 23;
            this.labelTriggerConditions.Text = "触发条件:";
            // 
            // lstTriggerConditions
            // 
            this.lstTriggerConditions = new System.Windows.Forms.ListBox();
            this.lstTriggerConditions.FormattingEnabled = true;
            this.lstTriggerConditions.ItemHeight = 17;
            this.lstTriggerConditions.Location = new System.Drawing.Point(94, 22);
            this.lstTriggerConditions.Name = "lstTriggerConditions";
            this.lstTriggerConditions.Size = new System.Drawing.Size(350, 44);
            this.lstTriggerConditions.TabIndex = 24;
            this.lstTriggerConditions.SelectedIndexChanged += new System.EventHandler(this.lstTriggerConditions_SelectedIndexChanged);
            // 
            // btnAddTriggerCondition
            // 
            this.btnAddTriggerCondition = new System.Windows.Forms.Button();
            this.btnAddTriggerCondition.Location = new System.Drawing.Point(450, 22);
            this.btnAddTriggerCondition.Name = "btnAddTriggerCondition";
            this.btnAddTriggerCondition.Size = new System.Drawing.Size(30, 23);
            this.btnAddTriggerCondition.TabIndex = 25;
            this.btnAddTriggerCondition.Text = "+";
            this.btnAddTriggerCondition.UseVisualStyleBackColor = true;
            this.btnAddTriggerCondition.Click += new System.EventHandler(this.btnAddTriggerCondition_Click);
            // 
            // btnEditTriggerCondition
            // 
            this.btnEditTriggerCondition = new System.Windows.Forms.Button();
            this.btnEditTriggerCondition.Enabled = false;
            this.btnEditTriggerCondition.Location = new System.Drawing.Point(450, 50);
            this.btnEditTriggerCondition.Name = "btnEditTriggerCondition";
            this.btnEditTriggerCondition.Size = new System.Drawing.Size(30, 23);
            this.btnEditTriggerCondition.TabIndex = 26;
            this.btnEditTriggerCondition.Text = "✎";
            this.btnEditTriggerCondition.UseVisualStyleBackColor = true;
            this.btnEditTriggerCondition.Click += new System.EventHandler(this.btnEditTriggerCondition_Click);
            // 
            // btnDeleteTriggerCondition
            // 
            this.btnDeleteTriggerCondition = new System.Windows.Forms.Button();
            this.btnDeleteTriggerCondition.Enabled = false;
            this.btnDeleteTriggerCondition.Location = new System.Drawing.Point(486, 50);
            this.btnDeleteTriggerCondition.Name = "btnDeleteTriggerCondition";
            this.btnDeleteTriggerCondition.Size = new System.Drawing.Size(30, 23);
            this.btnDeleteTriggerCondition.TabIndex = 27;
            this.btnDeleteTriggerCondition.Text = "-";
            this.btnDeleteTriggerCondition.UseVisualStyleBackColor = true;
            this.btnDeleteTriggerCondition.Click += new System.EventHandler(this.btnDeleteTriggerCondition_Click);
            // 
            // labelStatusConditions
            // 
            this.labelStatusConditions = new System.Windows.Forms.Label();
            this.labelStatusConditions.AutoSize = true;
            this.labelStatusConditions.Location = new System.Drawing.Point(6, 80);
            this.labelStatusConditions.Name = "labelStatusConditions";
            this.labelStatusConditions.Size = new System.Drawing.Size(76, 17);
            this.labelStatusConditions.TabIndex = 28;
            this.labelStatusConditions.Text = "状态条件:";
            // 
            // lstStatusConditions
            // 
            this.lstStatusConditions = new System.Windows.Forms.ListBox();
            this.lstStatusConditions.FormattingEnabled = true;
            this.lstStatusConditions.ItemHeight = 17;
            this.lstStatusConditions.Location = new System.Drawing.Point(94, 77);
            this.lstStatusConditions.Name = "lstStatusConditions";
            this.lstStatusConditions.Size = new System.Drawing.Size(350, 44);
            this.lstStatusConditions.TabIndex = 29;
            // 
            // btnAddStatusCondition
            // 
            this.btnAddStatusCondition = new System.Windows.Forms.Button();
            this.btnAddStatusCondition.Location = new System.Drawing.Point(450, 77);
            this.btnAddStatusCondition.Name = "btnAddStatusCondition";
            this.btnAddStatusCondition.Size = new System.Drawing.Size(30, 23);
            this.btnAddStatusCondition.TabIndex = 30;
            this.btnAddStatusCondition.Text = "+";
            this.btnAddStatusCondition.UseVisualStyleBackColor = true;
            this.btnAddStatusCondition.Click += new System.EventHandler(this.btnAddStatusCondition_Click);
            // 
            // btnEditStatusCondition
            // 
            this.btnEditStatusCondition = new System.Windows.Forms.Button();
            this.btnEditStatusCondition.Enabled = false;
            this.btnEditStatusCondition.Location = new System.Drawing.Point(450, 105);
            this.btnEditStatusCondition.Name = "btnEditStatusCondition";
            this.btnEditStatusCondition.Size = new System.Drawing.Size(30, 23);
            this.btnEditStatusCondition.TabIndex = 31;
            this.btnEditStatusCondition.Text = "✎";
            this.btnEditStatusCondition.UseVisualStyleBackColor = true;
            this.btnEditStatusCondition.Click += new System.EventHandler(this.btnEditStatusCondition_Click);
            // 
            // btnDeleteStatusCondition
            // 
            this.btnDeleteStatusCondition = new System.Windows.Forms.Button();
            this.btnDeleteStatusCondition.Enabled = false;
            this.btnDeleteStatusCondition.Location = new System.Drawing.Point(486, 105);
            this.btnDeleteStatusCondition.Name = "btnDeleteStatusCondition";
            this.btnDeleteStatusCondition.Size = new System.Drawing.Size(30, 23);
            this.btnDeleteStatusCondition.TabIndex = 32;
            this.btnDeleteStatusCondition.Text = "-";
            this.btnDeleteStatusCondition.UseVisualStyleBackColor = true;
            this.btnDeleteStatusCondition.Click += new System.EventHandler(this.btnDeleteStatusCondition_Click);
            // 将事件绑定移到所有相关控件创建完成之后
            this.lstStatusConditions.SelectedIndexChanged += new System.EventHandler(this.lstStatusConditions_SelectedIndexChanged);
            // 
            // labelActionConditions
            // 
            this.labelActionConditions = new System.Windows.Forms.Label();
            this.labelActionConditions.AutoSize = true;
            this.labelActionConditions.Location = new System.Drawing.Point(6, 135);
            this.labelActionConditions.Name = "labelActionConditions";
            this.labelActionConditions.Size = new System.Drawing.Size(76, 17);
            this.labelActionConditions.TabIndex = 33;
            this.labelActionConditions.Text = "动作条件:";
            // 
            // lstActionConditions
            // 
            this.lstActionConditions = new System.Windows.Forms.ListBox();
            this.lstActionConditions.FormattingEnabled = true;
            this.lstActionConditions.ItemHeight = 17;
            this.lstActionConditions.Location = new System.Drawing.Point(94, 132);
            this.lstActionConditions.Name = "lstActionConditions";
            this.lstActionConditions.Size = new System.Drawing.Size(350, 44);
            this.lstActionConditions.TabIndex = 34;
            // 
            // btnAddActionCondition
            // 
            this.btnAddActionCondition = new System.Windows.Forms.Button();
            this.btnAddActionCondition.Location = new System.Drawing.Point(450, 132);
            this.btnAddActionCondition.Name = "btnAddActionCondition";
            this.btnAddActionCondition.Size = new System.Drawing.Size(30, 23);
            this.btnAddActionCondition.TabIndex = 35;
            this.btnAddActionCondition.Text = "+";
            this.btnAddActionCondition.UseVisualStyleBackColor = true;
            this.btnAddActionCondition.Click += new System.EventHandler(this.btnAddActionCondition_Click);
            // 
            // btnEditActionCondition
            // 
            this.btnEditActionCondition = new System.Windows.Forms.Button();
            this.btnEditActionCondition.Enabled = false;
            this.btnEditActionCondition.Location = new System.Drawing.Point(450, 160);
            this.btnEditActionCondition.Name = "btnEditActionCondition";
            this.btnEditActionCondition.Size = new System.Drawing.Size(30, 23);
            this.btnEditActionCondition.TabIndex = 36;
            this.btnEditActionCondition.Text = "✎";
            this.btnEditActionCondition.UseVisualStyleBackColor = true;
            this.btnEditActionCondition.Click += new System.EventHandler(this.btnEditActionCondition_Click);
            // 
            // btnDeleteActionCondition
            // 
            this.btnDeleteActionCondition = new System.Windows.Forms.Button();
            this.btnDeleteActionCondition.Enabled = false;
            this.btnDeleteActionCondition.Location = new System.Drawing.Point(486, 160);
            this.btnDeleteActionCondition.Name = "btnDeleteActionCondition";
            this.btnDeleteActionCondition.Size = new System.Drawing.Size(30, 23);
            this.btnDeleteActionCondition.TabIndex = 37;
            this.btnDeleteActionCondition.Text = "-";
            this.btnDeleteActionCondition.UseVisualStyleBackColor = true;
            this.btnDeleteActionCondition.Click += new System.EventHandler(this.btnDeleteActionCondition_Click);
            // 将事件绑定移到所有相关控件创建完成之后
            this.lstActionConditions.SelectedIndexChanged += new System.EventHandler(this.lstActionConditions_SelectedIndexChanged);
            // 
            // grpOtherConfig
            // 
            this.grpOtherConfig.Controls.Add(this.labelTriggerConditions);
            this.grpOtherConfig.Controls.Add(this.lstTriggerConditions);
            this.grpOtherConfig.Controls.Add(this.btnAddTriggerCondition);
            this.grpOtherConfig.Controls.Add(this.btnEditTriggerCondition);
            this.grpOtherConfig.Controls.Add(this.btnDeleteTriggerCondition);
            this.grpOtherConfig.Controls.Add(this.labelStatusConditions);
            this.grpOtherConfig.Controls.Add(this.lstStatusConditions);
            this.grpOtherConfig.Controls.Add(this.btnAddStatusCondition);
            this.grpOtherConfig.Controls.Add(this.btnEditStatusCondition);
            this.grpOtherConfig.Controls.Add(this.btnDeleteStatusCondition);
            this.grpOtherConfig.Controls.Add(this.labelActionConditions);
            this.grpOtherConfig.Controls.Add(this.lstActionConditions);
            this.grpOtherConfig.Controls.Add(this.btnAddActionCondition);
            this.grpOtherConfig.Controls.Add(this.btnEditActionCondition);
            this.grpOtherConfig.Controls.Add(this.btnDeleteActionCondition);
            this.grpOtherConfig.Location = new System.Drawing.Point(12, 280);
            this.grpOtherConfig.Name = "grpOtherConfig";
            this.grpOtherConfig.Size = new System.Drawing.Size(530, 280); // 再次增大高度以确保所有控件完全可见
            this.grpOtherConfig.TabIndex = 38;
            this.grpOtherConfig.TabStop = false;
            this.grpOtherConfig.Text = "条件配置";
            // 
            // StrategyConfigForm
            // 
            this.ClientSize = new System.Drawing.Size(554, 620); // 进一步增加表单高度以适应更大的配置区域
            // 调整控件添加顺序，确保策略ID相关控件和启用策略复选框在容器控件之前添加
            // 这样可以利用BringToFront方法确保它们显示在最顶层
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtStrategyId);
            this.Controls.Add(this.btnGenerateId);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.grpDateConfig);
            this.Controls.Add(this.grpTimeConfig);
            this.Controls.Add(this.grpWeekConfig);
            // 移除对grpLogicConfig的引用，避免布局问题
            this.Controls.Add(this.grpOtherConfig);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            
            // 确保相关控件在最顶层
            this.label1.BringToFront();
            this.txtStrategyId.BringToFront();
            this.btnGenerateId.BringToFront();
            this.chkEnabled.BringToFront();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StrategyConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "新建策略";
            this.grpDateConfig.ResumeLayout(false);
            this.grpDateConfig.PerformLayout();
            this.grpTimeConfig.ResumeLayout(false);
            this.grpTimeConfig.PerformLayout();
            this.grpWeekConfig.ResumeLayout(false);
            this.grpWeekConfig.PerformLayout();
            // grpLogicConfig已移除，无需ResumeLayout
            this.grpOtherConfig.ResumeLayout(false);
            this.grpOtherConfig.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeUI()
        {
            // 初始化UI的额外设置
            // 设置默认选中的选项
            if (chkLogicOr != null) chkLogicOr.Checked = true;
            
            // 初始化控件可见性
            UpdateWeekControlsVisibility();
            UpdateHolidayControlsVisibility();
        }
        
        // 更新星期控件可见性
        private void UpdateWeekControlsVisibility()
        {            
            if (chkWeekMode != null)
            {
                bool isWeekModeChecked = chkWeekMode.Checked;
                if (label6 != null) label6.Visible = isWeekModeChecked;
                if (chkSunday != null) chkSunday.Visible = isWeekModeChecked;
                if (chkMonday != null) chkMonday.Visible = isWeekModeChecked;
                if (chkTuesday != null) chkTuesday.Visible = isWeekModeChecked;
                if (chkWednesday != null) chkWednesday.Visible = isWeekModeChecked;
                if (chkThursday != null) chkThursday.Visible = isWeekModeChecked;
                if (chkFriday != null) chkFriday.Visible = isWeekModeChecked;
                if (chkSaturday != null) chkSaturday.Visible = isWeekModeChecked;
            }
        }
        
        // 更新节假日控件可见性
        private void UpdateHolidayControlsVisibility()
        {
            if (chkHolidayWorkday != null && chkHolidayMode != null)
                chkHolidayWorkday.Visible = chkHolidayMode.Checked;
        }
        
        // 星期模式勾选状态变化处理
        private void chkWeekMode_CheckedChanged(object? sender, EventArgs e) // 将sender参数标记为可空，与EventHandler委托匹配
        {
            UpdateWeekControlsVisibility();
        }
        
        // 节假日模式勾选状态变化处理
        private void chkHolidayMode_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateHolidayControlsVisibility();
        }

        private uint GenerateRandomStrategyId()
        {            
            return (uint)new Random().Next(int.MinValue, int.MaxValue);
        }

        private void btnGenerateId_Click(object? sender, EventArgs e) // 将sender参数标记为可空，与EventHandler委托匹配
        {
            // 生成随机4字节hex ID并填充到文本框
            uint randomId = GenerateRandomStrategyId();
            if (txtStrategyId != null) txtStrategyId.Text = randomId.ToString("X8");
            if (StrategyData != null) StrategyData.StrategyId = randomId;
        }

        private void LoadStrategyData()
        {
            if (StrategyData == null) return;
            
            // 加载策略ID
            if (txtStrategyId != null)
            {
                txtStrategyId.Text = StrategyData.StrategyId.ToString("X8");
            }

            // 加载日期和时间
            if (dtpStartDate != null && StrategyData.StartDate != null) dtpStartDate.Value = ConvertBcdToDateTime(StrategyData.StartDate);
            if (dtpEndDate != null && StrategyData.EndDate != null) dtpEndDate.Value = ConvertBcdToDateTime(StrategyData.EndDate);
            if (dtpStartTime != null && StrategyData.StartTime != null) dtpStartTime.Value = ConvertBcdToTime(StrategyData.StartTime);
            if (dtpEndTime != null && StrategyData.EndTime != null) dtpEndTime.Value = ConvertBcdToTime(StrategyData.EndTime);

            // 解析星期-节假日策略
            ushort weekHolidayData = StrategyData.WeekHolidayData;
            
            // 加载星期选择
            if (chkSunday != null) chkSunday.Checked = (weekHolidayData & 0x01) != 0;
            if (chkMonday != null) chkMonday.Checked = (weekHolidayData & 0x02) != 0;
            if (chkTuesday != null) chkTuesday.Checked = (weekHolidayData & 0x04) != 0;
            if (chkWednesday != null) chkWednesday.Checked = (weekHolidayData & 0x08) != 0;
            if (chkThursday != null) chkThursday.Checked = (weekHolidayData & 0x10) != 0;
            if (chkFriday != null) chkFriday.Checked = (weekHolidayData & 0x20) != 0;
            if (chkSaturday != null) chkSaturday.Checked = (weekHolidayData & 0x40) != 0;
            
            // 加载模式选择
            if (chkWeekMode != null) chkWeekMode.Checked = (weekHolidayData & 0x80) != 0;
            if (chkHolidayMode != null) chkHolidayMode.Checked = (weekHolidayData & 0x100) != 0;
            if (chkHolidayWorkday != null) chkHolidayWorkday.Checked = (weekHolidayData & 0x200) != 0;
            if (chkLogicAnd != null) chkLogicAnd.Checked = (weekHolidayData & 0x400) != 0;
            if (chkLogicOr != null) chkLogicOr.Checked = (weekHolidayData & 0x400) == 0;

            // 加载条件列表
            LoadConditionLists();

            // 加载使能状态
            if (chkEnabled != null) chkEnabled.Checked = (StrategyData.EnabledFlag & 0x01) != 0;
        }

        private void LoadConditionLists()
        {
            // 清空现有列表，使用null条件操作符避免空引用
            lstTriggerConditions?.Items?.Clear();
            lstStatusConditions?.Items?.Clear();
            lstActionConditions?.Items?.Clear();

            // 加载触发条件
            if (StrategyData != null) // 添加StrategyData的null检查
            {                
                // 加载触发条件
                if (StrategyData.TriggerConditions != null)
                {                    
                    foreach (var condition in StrategyData.TriggerConditions)
                    {                      
                        if (condition != null) lstTriggerConditions?.Items?.Add(condition);
                    }
                }
                
                // 加载状态条件
                if (StrategyData.StatusConditions != null)
                {                    
                    foreach (var condition in StrategyData.StatusConditions)
                    {
                        if (condition != null) lstStatusConditions?.Items?.Add(condition);
                    }
                }
                
                // 加载动作条件
                if (StrategyData.ActionConditions != null)
                {                    
                    foreach (var condition in StrategyData.ActionConditions)
                    {
                        if (condition != null && lstActionConditions != null && lstActionConditions.Items != null)
                        {
                            lstActionConditions.Items.Add(condition);
                        }
                    }
                }
            }
        }

        // 触发条件列表选中项变化
        private void lstTriggerConditions_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isItemSelected = lstTriggerConditions != null && lstTriggerConditions.SelectedIndex >= 0;
            if (btnEditTriggerCondition != null) btnEditTriggerCondition.Enabled = isItemSelected;
            if (btnDeleteTriggerCondition != null) btnDeleteTriggerCondition.Enabled = isItemSelected;
        }

        // 状态条件列表选中项变化
        private void lstStatusConditions_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isItemSelected = lstStatusConditions != null && lstStatusConditions.SelectedIndex >= 0;
            if (btnEditStatusCondition != null) btnEditStatusCondition.Enabled = isItemSelected;
            if (btnDeleteStatusCondition != null) btnDeleteStatusCondition.Enabled = isItemSelected;
        }

        // 动作条件列表选中项变化
        private void lstActionConditions_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isItemSelected = lstActionConditions != null && lstActionConditions.SelectedIndex >= 0;
            if (btnEditActionCondition != null) btnEditActionCondition.Enabled = isItemSelected;
            if (btnDeleteActionCondition != null) btnDeleteActionCondition.Enabled = isItemSelected;
        }

        // 添加触发条件
        private void btnAddTriggerCondition_Click(object? sender, EventArgs e)
        {
            ConditionEditForm editForm = new ConditionEditForm(isTriggerOrStatusCondition: true);
            if (editForm.ShowDialog() == DialogResult.OK && editForm.TriggerCondition != null)
            {
                if (StrategyData != null && StrategyData.TriggerConditions != null)
                {
                    StrategyData.TriggerConditions.Add(editForm.TriggerCondition);
                    lstTriggerConditions?.Items?.Add(editForm.TriggerCondition);
                }
            }
        }

        // 编辑触发条件
        private void btnEditTriggerCondition_Click(object? sender, EventArgs e)
        {
            if (lstTriggerConditions != null && lstTriggerConditions.SelectedIndex >= 0)
            {
                TriggerCondition selectedCondition = lstTriggerConditions.SelectedItem as TriggerCondition ?? new TriggerCondition(); // 安全转换
                ConditionEditForm editForm = new ConditionEditForm(triggerCondition: selectedCondition, isTriggerOrStatusCondition: true);
                if (editForm.ShowDialog() == DialogResult.OK && editForm.TriggerCondition != null)
                {
                    // 更新列表中的项
                    if (StrategyData != null && StrategyData.TriggerConditions != null && 
                        lstTriggerConditions.SelectedIndex >= 0 && lstTriggerConditions.SelectedIndex < StrategyData.TriggerConditions.Count)
                    {
                        StrategyData.TriggerConditions[lstTriggerConditions.SelectedIndex] = editForm.TriggerCondition;
                        if (lstTriggerConditions != null && lstTriggerConditions.Items != null && lstTriggerConditions.SelectedIndex >= 0 && lstTriggerConditions.SelectedIndex < lstTriggerConditions.Items.Count)
                        {
                            lstTriggerConditions.Items[lstTriggerConditions.SelectedIndex] = editForm.TriggerCondition;
                        }
                    }
                }
            }
        }

        // 删除触发条件
        private void btnDeleteTriggerCondition_Click(object? sender, EventArgs e)
        {
            if (lstTriggerConditions != null && lstTriggerConditions.SelectedIndex >= 0)
            {
                if (MessageBox.Show("确定要删除选中的触发条件吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (StrategyData != null && StrategyData.TriggerConditions != null && 
                        lstTriggerConditions.SelectedIndex >= 0 && lstTriggerConditions.SelectedIndex < StrategyData.TriggerConditions.Count)
                    {
                        StrategyData.TriggerConditions.RemoveAt(lstTriggerConditions.SelectedIndex);
                        lstTriggerConditions?.Items?.RemoveAt(lstTriggerConditions.SelectedIndex);
                    }
                }
            }
        }

        // 添加状态条件
        private void btnAddStatusCondition_Click(object? sender, EventArgs e)
        {
            ConditionEditForm editForm = new ConditionEditForm(statusCondition: new StatusCondition(), isTriggerOrStatusCondition: true);
            editForm.Text = "新建状态条件";
            if (editForm.ShowDialog() == DialogResult.OK && editForm.StatusCondition != null)
            {
                if (StrategyData != null && StrategyData.StatusConditions != null)
                {
                    StrategyData.StatusConditions.Add(editForm.StatusCondition);
                    lstStatusConditions?.Items?.Add(editForm.StatusCondition);
                }
            }
        }

        // 编辑状态条件
        private void btnEditStatusCondition_Click(object? sender, EventArgs e)
        {
            if (lstStatusConditions != null && lstStatusConditions.SelectedIndex >= 0)
            {
                StatusCondition selectedCondition = lstStatusConditions.SelectedItem as StatusCondition ?? new StatusCondition(); // 安全转换
                ConditionEditForm editForm = new ConditionEditForm(statusCondition: selectedCondition, isTriggerOrStatusCondition: true);
                editForm.Text = "编辑状态条件";
                if (editForm.ShowDialog() == DialogResult.OK && editForm.StatusCondition != null)
                {
                    // 更新列表中的项
                    if (StrategyData != null && StrategyData.StatusConditions != null && 
                        lstStatusConditions.SelectedIndex >= 0 && lstStatusConditions.SelectedIndex < StrategyData.StatusConditions.Count)
                    {
                        StrategyData.StatusConditions[lstStatusConditions.SelectedIndex] = editForm.StatusCondition;
                        if (lstStatusConditions != null && lstStatusConditions.Items != null && lstStatusConditions.SelectedIndex >= 0 && lstStatusConditions.SelectedIndex < lstStatusConditions.Items.Count)
                        {
                            lstStatusConditions.Items[lstStatusConditions.SelectedIndex] = editForm.StatusCondition;
                        }
                    }
                }
            }
        }

        // 删除状态条件
        private void btnDeleteStatusCondition_Click(object? sender, EventArgs e)
        {
            if (lstStatusConditions != null && lstStatusConditions.SelectedIndex >= 0)
            {
                if (MessageBox.Show("确定要删除选中的状态条件吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (StrategyData != null && StrategyData.StatusConditions != null && 
                        lstStatusConditions.SelectedIndex >= 0 && lstStatusConditions.SelectedIndex < StrategyData.StatusConditions.Count)
                    {
                        StrategyData.StatusConditions.RemoveAt(lstStatusConditions.SelectedIndex);
                        lstStatusConditions?.Items?.RemoveAt(lstStatusConditions.SelectedIndex);
                    }
                }
            }
        }

        // 添加动作条件
        private void btnAddActionCondition_Click(object? sender, EventArgs e)
        {
            // 使用动作条件专用构造函数
            ConditionEditForm editForm = new ConditionEditForm(new ActionCondition(), true);
            if (editForm.ShowDialog() == DialogResult.OK && editForm.ActionCondition != null)
            {
                if (StrategyData != null && StrategyData.ActionConditions != null)
                {
                    StrategyData.ActionConditions.Add(editForm.ActionCondition);
                    lstActionConditions?.Items?.Add(editForm.ActionCondition);
                }
            }
        }

        // 编辑动作条件
        private void btnEditActionCondition_Click(object? sender, EventArgs e)
        {
            if (lstActionConditions != null && lstActionConditions.SelectedIndex >= 0)
            {
                ActionCondition selectedCondition = lstActionConditions.SelectedItem as ActionCondition ?? new ActionCondition(); // 安全转换
                // 使用动作条件专用构造函数
                ConditionEditForm editForm = new ConditionEditForm(selectedCondition, true);
                if (editForm.ShowDialog() == DialogResult.OK && editForm.ActionCondition != null)
                {
                    // 更新列表中的项
                    if (StrategyData != null && lstActionConditions.SelectedIndex >= 0 && lstActionConditions.SelectedIndex < StrategyData.ActionConditions.Count)
                    {
                        StrategyData.ActionConditions[lstActionConditions.SelectedIndex] = editForm.ActionCondition;
                        if (lstActionConditions != null && lstActionConditions.Items != null && lstActionConditions.SelectedIndex >= 0 && lstActionConditions.SelectedIndex < lstActionConditions.Items.Count)
                        {
                            lstActionConditions.Items[lstActionConditions.SelectedIndex] = editForm.ActionCondition;
                        }
                    }
                }
            }
        }

        // 删除动作条件
        private void btnDeleteActionCondition_Click(object? sender, EventArgs e)
        {
            if (lstActionConditions != null && lstActionConditions.SelectedIndex >= 0)
            {
                if (MessageBox.Show("确定要删除选中的动作条件吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (StrategyData != null && StrategyData.ActionConditions != null && 
                        lstActionConditions.SelectedIndex >= 0 && lstActionConditions.SelectedIndex < StrategyData.ActionConditions.Count)
                    {
                        StrategyData.ActionConditions.RemoveAt(lstActionConditions.SelectedIndex);
                        lstActionConditions?.Items?.RemoveAt(lstActionConditions.SelectedIndex);
                    }
                }
            }
        }

        private void SaveStrategyData()
        {
            // 添加StrategyData的null检查
            if (StrategyData != null)
            {
                // 保存策略ID
                if (txtStrategyId != null && !string.IsNullOrEmpty(txtStrategyId.Text.Trim()) && 
                    uint.TryParse(txtStrategyId.Text.Trim(), System.Globalization.NumberStyles.HexNumber, null, out uint strategyId))
                {
                    StrategyData.StrategyId = strategyId;
                }

                // 保存日期和时间（BCD格式）
                if (dtpStartDate != null) StrategyData.StartDate = ConvertDateTimeToBcd(dtpStartDate.Value);
                if (dtpEndDate != null) StrategyData.EndDate = ConvertDateTimeToBcd(dtpEndDate.Value);
                if (dtpStartTime != null) StrategyData.StartTime = ConvertTimeToBcd(dtpStartTime.Value);
                if (dtpEndTime != null) StrategyData.EndTime = ConvertTimeToBcd(dtpEndTime.Value);

                // 构建星期-节假日策略
                ushort weekHolidayData = 0;
                
                // 添加星期选择
                if (chkSunday?.Checked ?? false) weekHolidayData |= 0x01;
                if (chkMonday?.Checked ?? false) weekHolidayData |= 0x02;
                if (chkTuesday?.Checked ?? false) weekHolidayData |= 0x04;
                if (chkWednesday?.Checked ?? false) weekHolidayData |= 0x08;
                if (chkThursday?.Checked ?? false) weekHolidayData |= 0x10;
                if (chkFriday?.Checked ?? false) weekHolidayData |= 0x20;
                if (chkSaturday?.Checked ?? false) weekHolidayData |= 0x40;
                
                // 添加模式选择
                if (chkWeekMode?.Checked ?? false) weekHolidayData |= 0x80;
                if (chkHolidayMode?.Checked ?? false) weekHolidayData |= 0x100;
                if (chkHolidayWorkday?.Checked ?? false) weekHolidayData |= 0x200;
                if (chkLogicAnd?.Checked ?? false) weekHolidayData |= 0x400;
                
                StrategyData.WeekHolidayData = weekHolidayData;

                // 使能状态已通过列表自动更新，无需单独设置计数
                StrategyData.EnabledFlag = (byte)((chkEnabled?.Checked ?? false) ? 0x01 : 0x00);
            }
        }

        // BCD转换辅助方法
        private byte[] ConvertDateTimeToBcd(DateTime dateTime)
        {
            byte[] bcdDate = new byte[3];
            // 年份：取最后两位，如2025 -> 0x25
            bcdDate[0] = (byte)((dateTime.Year % 100 / 10 << 4) | (dateTime.Year % 10));
            // 月份：如11月 -> 0x0B
            bcdDate[1] = (byte)((dateTime.Month / 10 << 4) | (dateTime.Month % 10));
            // 日期：如19日 -> 0x13
            bcdDate[2] = (byte)((dateTime.Day / 10 << 4) | (dateTime.Day % 10));
            return bcdDate;
        }

        private byte[] ConvertTimeToBcd(DateTime dateTime)
        {
            byte[] bcdTime = new byte[2];
            // 小时：如19时 -> 0x19
            bcdTime[0] = (byte)((dateTime.Hour / 10 << 4) | (dateTime.Hour % 10));
            // 分钟：如30分 -> 0x30
            bcdTime[1] = (byte)((dateTime.Minute / 10 << 4) | (dateTime.Minute % 10));
            return bcdTime;
        }

        private DateTime ConvertBcdToDateTime(byte[] bcdDate)
        {
            if (bcdDate == null || bcdDate.Length != 3)
                throw new ArgumentException("BCD日期数组不能为null且长度必须为3");

            // 从BCD转换年份
            int year = 2000 + ((bcdDate[0] >> 4) * 10) + (bcdDate[0] & 0x0F);
            // 从BCD转换月份
            int month = ((bcdDate[1] >> 4) * 10) + (bcdDate[1] & 0x0F);
            // 从BCD转换日期
            int day = ((bcdDate[2] >> 4) * 10) + (bcdDate[2] & 0x0F);

            return new DateTime(year, month, day);
        }

        private DateTime ConvertBcdToTime(byte[] bcdTime)
        {
            if (bcdTime == null || bcdTime.Length != 2)
                throw new ArgumentException("BCD时间数组不能为null且长度必须为2");

            // 从BCD转换小时
            int hour = ((bcdTime[0] >> 4) * 10) + (bcdTime[0] & 0x0F);
            // 从BCD转换分钟
            int minute = ((bcdTime[1] >> 4) * 10) + (bcdTime[1] & 0x0F);

            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, 0);
        }

        private void btnOK_Click(object? sender, EventArgs e)
        {
            try
            {
                SaveStrategyData();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存策略数据失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void chkLogicAnd_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Checked && chkLogicOr != null)
                chkLogicOr.Checked = false;
        }

        private void chkLogicOr_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Checked && chkLogicAnd != null)
                chkLogicAnd.Checked = false;
        }
    }
}