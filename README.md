# MyAITools

## 自用AI工具集

### 1. 介绍

1. 本项目是一个自用的AI工具集，包含了一些常用的AI工具。
2. 本项目的目的是为了方便自己的工作。

### 2. 项目结构

1. 本项目分为两部分：本地客户端和服务器端；
2. 本地代码采用.net 8.0框架编写，MAUI-blazor。目前通过调用微软SK，通过调用OPENAI的api接口服务实现AI功能；——后续可能会更改为avalonia，maui-blazor感觉有点不方便；
3. 本地客户端目前只在Windows系统测试过；
4. 服务器端采用Ollama，目前只是一个简单的服务端，用于RAG问答；目前没有代码，只是本地简单跑了一下ollama；
5. phi3:medium-128k用于chat模型，nomic-embed-text用于embed模型；

### 3. 具体功能

1. 聊天工具：用户可以和机器人进行聊天，机器人会根据用户输入的内容进行回答，能够自动调取插件和使用记忆功能，并且可以输出图片；增加了搜索引擎等功能；
2. 插件工具：目前主要用于测试各类插件；
