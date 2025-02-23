#!/bin/zsh

source ~/.zshrc

ollama rm doxypatch
ollama create doxypatch -f doxypatch
ollama rm doxypatch-with-context
ollama create doxypatch-with-context -f doxypatch-with-context
