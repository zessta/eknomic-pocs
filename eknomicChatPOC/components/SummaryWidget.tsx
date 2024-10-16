import React from 'react';
import { View, Text, Button, StyleSheet } from 'react-native';
import axios from 'axios';
import { IMessage } from 'react-native-gifted-chat';

interface SummaryWidgetProps {
  messages: IMessage[];
  onYes: () => void;
  onNo: () => void;
  onReceiveSummary: (summary: string) => void; // New prop
}

const SummaryWidget: React.FC<SummaryWidgetProps> = ({ messages, onYes, onNo, onReceiveSummary }) => {
  const getSummary = () => {
    return 'Do you want summary of last 10 messages?';
  };

  const handleYes = async () => {
    const messageTexts = messages.map(msg => msg.text).join(' | ');
    console.log('messageTexts', messageTexts)
    try {
      const response = await axios.post('http://127.0.0.1:8000/gpt-response', {
        text: messageTexts,
      });

      console.log('API Response:', response.data);
      onReceiveSummary(response.data); // Pass the summary back to ChatScreen
    } catch (error) {
      console.error('Error fetching summary:', error.message);
    if (error.response) {
      // The request was made and the server responded with a status code
      console.error('Response data:', error.response.data);
      console.error('Response status:', error.response.status);
    } else if (error.request) {
      // The request was made but no response was received
      console.error('Request data:', error.request);
    }
  }
    onYes(); // Call the onYes callback after making the API call
  };

  return (
    <View style={styles.container}>
      <Text style={styles.summaryText}>{getSummary()}</Text>
      <View style={styles.buttonContainer}>
        <Button title="Yes" onPress={handleYes} />
        <Button title="No" onPress={onNo} />
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    padding: 10,
    backgroundColor: '#f8f8f8',
    borderRadius: 10,
    marginVertical: 10,
  },
  summaryText: {
    marginBottom: 10,
    fontSize: 16,
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    gap: 10,
  },
});

export default SummaryWidget;
